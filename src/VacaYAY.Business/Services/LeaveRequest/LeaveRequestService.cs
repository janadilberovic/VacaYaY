using Mapster;
using Microsoft.EntityFrameworkCore;
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

    public async Task<IReadOnlyList<LeaveRequestDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var requests = await WithNavigations(_db.LeaveRequests.AsNoTracking())
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(ToDto).ToList();
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetMineAsync(int employeeId, CancellationToken cancellationToken = default)
    {
        var requests = await WithNavigations(_db.LeaveRequests.AsNoTracking())
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(ToDto).ToList();
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

        bool typeExists = await _db.LeaveTypes.AnyAsync(t => t.Id == request.LeaveTypeId, cancellationToken);
        if (!typeExists) { return CreateLeaveRequestResult.LeaveTypeNotFound; }

        // Reject a range that intersects one of the employee's pending/approved requests.
        bool overlaps = await _db.LeaveRequests.AnyAsync(r =>
            r.EmployeeId == employeeId &&
            (r.Status == LeaveRequestStatus.Pending || r.Status == LeaveRequestStatus.Approved) &&
            r.StartDate <= request.EndDate && r.EndDate >= request.StartDate,
            cancellationToken); 
        if (overlaps) { return CreateLeaveRequestResult.Overlap; }

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

    //helper da vracam zaposlene i leavetype
    private static IQueryable<LeaveRequestEntity> WithNavigations(IQueryable<LeaveRequestEntity> query) =>
        query.Include(r => r.Employee).Include(r => r.LeaveType);

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
