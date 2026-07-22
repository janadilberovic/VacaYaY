using Mapster;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.Common;
using VacaYAY.Business.DTOs.LeaveRequest;
using VacaYAY.Business.Interfaces.LeaveRequest;
using VacaYAY.Data;
using LeaveRequestEntity = VacaYAY.Domain.Entities.LeaveRequest;

namespace VacaYAY.Business.Services.LeaveRequest;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly VacaYAYDbContext _db;
    private readonly IHolidayProvider _holidays;

    public LeaveRequestService(VacaYAYDbContext db, IHolidayProvider holidays)
    {
        _db = db;
        _holidays = holidays;
    }

    public async Task<PagedResult<LeaveRequestDto>> GetPagedAsync(GetLeaveRequestsRequest request, CancellationToken cancellationToken = default)
    {
        var query = ApplyFilters(WithNavigations(_db.LeaveRequests.AsNoTracking()), request);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await ApplySort(query, request)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LeaveRequestDto>
        {
            Items = items.Select(ToDto).ToList(),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };
    }

    public Task<PagedResult<LeaveRequestDto>> GetMinePagedAsync(int employeeId, GetLeaveRequestsRequest request, CancellationToken cancellationToken = default)
    {
        // The caller's id wins over anything the client sent, so nobody can page another employee's requests.
        request.EmployeeId = employeeId;
        return GetPagedAsync(request, cancellationToken);
    }

    public async Task<LeaveRequestSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var countByStatus = await _db.LeaveRequests
            .AsNoTracking()
            .GroupBy(r => r.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        // WorkingDays isn't a column, so the days-per-type totals need the dates in memory.
        var counted = await _db.LeaveRequests
            .AsNoTracking()
            .Where(r => r.Status == LeaveRequestStatus.Pending || r.Status == LeaveRequestStatus.Approved)
            .Select(r => new { r.LeaveTypeId, Name = r.LeaveType!.Name, r.StartDate, r.EndDate })
            .ToListAsync(cancellationToken);

        var daysByType = counted
            .GroupBy(r => new { r.LeaveTypeId, r.Name })
            .Select(g => new LeaveTypeDaysDto
            {
                LeaveTypeId = g.Key.LeaveTypeId,
                LeaveTypeName = g.Key.Name,
                WorkingDays = g.Sum(r => CountWorkingDays(r.StartDate, r.EndDate)),
            })
            .OrderByDescending(t => t.WorkingDays)
            .ToList();

        return new LeaveRequestSummaryDto
        {
            TotalCount = countByStatus.Sum(s => s.Count),
            CountByStatus = countByStatus.ToDictionary(s => s.Status.ToString(), s => s.Count),
            DaysByType = daysByType,
        };
    }

    public async Task<LeaveRequestDto?> GetByIdAsync(int id, int requestingUserId, UserRole role, CancellationToken cancellationToken = default)
    {
        var request = await WithNavigations(_db.LeaveRequests.AsNoTracking())
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (request is null) { return null; }

        // Employees only see their own; HR sees everything.
        if (role != UserRole.HR && request.EmployeeId != requestingUserId) { return null; }

        return ToDto(request);
    }

    public async Task<CreateLeaveRequestResult> CreateAsync(int employeeId, CreateLeaveRequestRequest request, CancellationToken cancellationToken = default)
    {
        var leaveType = await _db.LeaveTypes.AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == request.LeaveTypeId, cancellationToken);
        if (leaveType is null) { return CreateLeaveRequestResult.LeaveTypeNotFound; }

        // Reject a range that intersects one of the employee's pending/approved requests.
        bool overlaps = await _db.LeaveRequests.AnyAsync(r =>
            r.EmployeeId == employeeId &&
            (r.Status == LeaveRequestStatus.Pending || r.Status == LeaveRequestStatus.Approved) &&
            r.StartDate <= request.EndDate && r.EndDate >= request.StartDate,
            cancellationToken);
        if (overlaps) { return CreateLeaveRequestResult.Overlap; }

        if (leaveType.CountsAgainstBalance)
        {
            var balance = await GetBalanceAsync(employeeId, cancellationToken);
            int requestedDays = CountWorkingDays(request.StartDate, request.EndDate);

            if (requestedDays > balance.RemainingDays)
            {
                return CreateLeaveRequestResult.InsufficientBalance(requestedDays, balance.RemainingDays);
            }
        }

        var entity = request.Adapt<LeaveRequestEntity>();
        entity.EmployeeId = employeeId;
        entity.Status = LeaveRequestStatus.Pending;
        entity.CreatedAt = DateTime.UtcNow;

        _db.LeaveRequests.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        // Reload with navigations so the DTO carries employee/type names.
        var created = await WithNavigations(_db.LeaveRequests.AsNoTracking())
            .FirstAsync(r => r.Id == entity.Id, cancellationToken);

        return CreateLeaveRequestResult.Created(ToDto(created));
    }

    public async Task<ReviewLeaveRequestResult> ApproveAsync(int id, int hrUserId, ReviewLeaveRequestRequest request, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await WithNavigations(_db.LeaveRequests)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (leaveRequest is null) { return ReviewLeaveRequestResult.NotFound; }
        if (leaveRequest.Status != LeaveRequestStatus.Pending) { return ReviewLeaveRequestResult.NotPending; }

        bool countsAgainstBalance = leaveRequest.LeaveType is not null && leaveRequest.LeaveType.CountsAgainstBalance;
        int workingDays = countsAgainstBalance ? CountWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate) : 0;

        // Wrap the balance deduction and the status change so they commit (or roll back) together.
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        if (workingDays > 0)
        {
           
            int affected = await _db.Users
                .Where(u => u.Id == leaveRequest.EmployeeId && u.DaysOff >= workingDays)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(u => u.DaysOff, u => u.DaysOff - workingDays),
                    cancellationToken);

            if (affected == 0) { return ReviewLeaveRequestResult.InsufficientBalance; }
        }

        leaveRequest.Status = LeaveRequestStatus.Approved;
        leaveRequest.HrComment = request.HrComment;
        leaveRequest.HrName = await GetHrNameAsync(hrUserId, cancellationToken);
        leaveRequest.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ReviewLeaveRequestResult.Reviewed(ToDto(leaveRequest));
    }

    public async Task<ReviewLeaveRequestResult> RejectAsync(int id, int hrUserId, ReviewLeaveRequestRequest request, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await WithNavigations(_db.LeaveRequests)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        if (leaveRequest is null) { return ReviewLeaveRequestResult.NotFound; }
        if (leaveRequest.Status != LeaveRequestStatus.Pending) { return ReviewLeaveRequestResult.NotPending; }

        leaveRequest.Status = LeaveRequestStatus.Rejected;
        leaveRequest.HrComment = request.HrComment;
        leaveRequest.HrName = await GetHrNameAsync(hrUserId, cancellationToken);
        leaveRequest.ReviewedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        return ReviewLeaveRequestResult.Reviewed(ToDto(leaveRequest));
    }

    public async Task<ReviewLeaveRequestResult> CancelAsync(int id, int employeeId, CancellationToken cancellationToken = default)
    {
        var leaveRequest = await WithNavigations(_db.LeaveRequests)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

        // Don't reveal other employees' requests — treat as not found.
        if (leaveRequest is null || leaveRequest.EmployeeId != employeeId) { return ReviewLeaveRequestResult.NotFound; }

        int refundDays = 0;
        switch (leaveRequest.Status)
        {
            case LeaveRequestStatus.Pending:
                break;

            case LeaveRequestStatus.Approved:
                // Refund the days that were deducted on approval.
                if (leaveRequest.LeaveType is not null && leaveRequest.LeaveType.CountsAgainstBalance)
                {
                    refundDays = CountWorkingDays(leaveRequest.StartDate, leaveRequest.EndDate);
                }
                break;

            default: // Rejected or already Cancelled
                return ReviewLeaveRequestResult.NotPending;
        }

        // Wrap the refund and the status change so they commit (or roll back) together.
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

        if (refundDays > 0)
        {
            // Atomic increment against the current DB value, so a concurrent approval's deduction
            // isn't lost to a stale in-memory read.
            await _db.Users
                .Where(u => u.Id == leaveRequest.EmployeeId)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(u => u.DaysOff, u => u.DaysOff + refundDays),
                    cancellationToken);
        }

        leaveRequest.Status = LeaveRequestStatus.Cancelled;
        await _db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ReviewLeaveRequestResult.Reviewed(ToDto(leaveRequest));
    }

    public async Task<LeaveBalanceDto> GetBalanceAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        int daysOff = await _db.Users
            .Where(u => u.Id == employeeId)
            .Select(u => u.DaysOff)
            .FirstOrDefaultAsync(cancellationToken);

        // Approved requests are already deducted from DaysOff; only pending ones still need reserving.
        // WorkingDays isn't a column, so the dates come back in memory to be counted.
        var pending = await _db.LeaveRequests
            .AsNoTracking()
            .Where(r => r.EmployeeId == employeeId
                && r.Status == LeaveRequestStatus.Pending
                && r.LeaveType!.CountsAgainstBalance)
            .Select(r => new { r.StartDate, r.EndDate })
            .ToListAsync(cancellationToken);

        int pendingDays = pending.Sum(r => CountWorkingDays(r.StartDate, r.EndDate));

        return new LeaveBalanceDto
        {
            DaysOff = daysOff,
            PendingDays = pendingDays,
            RemainingDays = daysOff - pendingDays,
        };
    }

    public Task<IReadOnlyList<DateOnly>> GetHolidaysAsync(int year, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DateOnly> days = _holidays.ForYear(year).OrderBy(d => d).ToList();
        return Task.FromResult(days);
    }

    //helper da vracam zaposlene i leavetype
    private static IQueryable<LeaveRequestEntity> WithNavigations(IQueryable<LeaveRequestEntity> query) =>
        query.Include(r => r.Employee).Include(r => r.LeaveType);

    private static IQueryable<LeaveRequestEntity> ApplyFilters(IQueryable<LeaveRequestEntity> query, GetLeaveRequestsRequest request)
    {
        if (request.Status is not null)
        {
            query = query.Where(r => r.Status == request.Status);
        }

        if (request.EmployeeId is not null)
        {
            query = query.Where(r => r.EmployeeId == request.EmployeeId);
        }

        if (request.LeaveTypeName is not null)
        {
            query = query.Where(r => r.LeaveType!.Name == request.LeaveTypeName);
        }

        return query;
    }

    // Id is the tiebreaker on every branch — without it, equal keys shuffle between pages.
    private static IQueryable<LeaveRequestEntity> ApplySort(IQueryable<LeaveRequestEntity> query, GetLeaveRequestsRequest request)
    {
        bool desc = request.SortDescending;

        IOrderedQueryable<LeaveRequestEntity> sorted = request.SortBy switch
        {
            LeaveRequestSortField.StartDate => Order(query, r => r.StartDate, desc),
            LeaveRequestSortField.EndDate => Order(query, r => r.EndDate, desc),
            LeaveRequestSortField.CreatedAt => Order(query, r => r.CreatedAt, desc),
            LeaveRequestSortField.EmployeeName => Order(query, r => r.Employee!.LastName, desc)
                .ThenBy(r => r.Employee!.FirstName),
            LeaveRequestSortField.Status => Order(query, r => r.Status, desc),
            _ => query
                .OrderBy(r => r.Status == LeaveRequestStatus.Pending ? 0 : 1)
                .ThenByDescending(r => r.StartDate),
        };

        return sorted.ThenBy(r => r.Id);
    }

    private static IOrderedQueryable<LeaveRequestEntity> Order<TKey>(
        IQueryable<LeaveRequestEntity> query,
        System.Linq.Expressions.Expression<Func<LeaveRequestEntity, TKey>> key,
        bool descending) =>
        descending ? query.OrderByDescending(key) : query.OrderBy(key);

    private LeaveRequestDto ToDto(LeaveRequestEntity request)
    {
        var dto = request.Adapt<LeaveRequestDto>();
        dto.WorkingDays = CountWorkingDays(request.StartDate, request.EndDate);
        return dto;
    }

    // Working days in [start, end] inclusive, excluding weekends and Serbian public holidays.
    private int CountWorkingDays(DateTime start, DateTime end)
    {
        int count = 0;
        for (var day = DateOnly.FromDateTime(start); day <= DateOnly.FromDateTime(end); day = day.AddDays(1))
        {
            if (day.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday) { continue; }
            if (_holidays.IsHoliday(day)) { continue; }

            count++;
        }

        return count;
    }

    private async Task<string?> GetHrNameAsync(int hrUserId, CancellationToken cancellationToken)
    {
        var hr = await _db.Users.FirstOrDefaultAsync(u => u.Id == hrUserId, cancellationToken);
        return hr is null ? null : $"{hr.FirstName} {hr.LastName}";
    }
}
