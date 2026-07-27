using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.Services.LeaveRequest;
using VacaYAY.Data;
using VacaYAY.Domain.Entities;
using VacaYAY.Business.DTOs.LeaveRequest;

namespace VacaYAY.UnitTests;

public class LeaveRequestServiceTests : IDisposable
{
    private readonly VacaYAYDbContext _db;
    private readonly LeaveRequestService _service;

    public LeaveRequestServiceTests()
    {
        _db = NewDb();
        _service = new LeaveRequestService(_db, new SerbianHolidayProvider());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetBalanceAsync_SubtractsPendingWorkingDays_FromDaysOff()
    {
        // Given
        _db.Users.Add(new User { Id = 1, Email = "ana@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 1, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateTime(2026, 7, 27),
            EndDate = new DateTime(2026, 7, 31),
        });
        await _db.SaveChangesAsync();

        // When
        var balance = await _service.GetBalanceAsync(1);

        // Then
        Assert.Equal(20, balance.DaysOff);
        Assert.Equal(5, balance.PendingDays);
        Assert.Equal(15, balance.RemainingDays);
    }

    [Fact]
    public async Task GetBalanceAsync_IgnoresTypesThatDontCountAgainstBalance()
    {
        // Given
        _db.Users.Add(new User { Id = 2, Email = "jana@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 2, Name = LeaveTypeName.Annual, CountsAgainstBalance = false });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 2,
            EmployeeId = 2,
            LeaveTypeId = 2,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateTime(2026, 7, 27),
            EndDate = new DateTime(2026, 7, 31),
        });
        await _db.SaveChangesAsync();

        // When
        var balance = await _service.GetBalanceAsync(2);

        // Then
        Assert.Equal(20, balance.DaysOff);
        Assert.Equal(0, balance.PendingDays);
        Assert.Equal(20, balance.RemainingDays);
    }

    [Fact]
    public async Task CreateAsync_ReturnsLeaveTypeNotFound()
    {
        // Given
        var request = new CreateLeaveRequestRequest
        {
            LeaveTypeId = 999,
            StartDate = new DateTime(2026, 7, 24),
            EndDate = new DateTime(2026, 7, 30)
        };

        // When
        var result = await _service.CreateAsync(1, request);

        // Then
        Assert.Equal(CreateLeaveRequestStatus.LeaveTypeNotFound, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ReturnsInsufficientBalance_WhenRequestExceedsRemaining()
    {
        // Given
        _db.Users.Add(new User { Id = 3, Email = "jjana@vacayay.com", DaysOff = 2 });
        _db.LeaveTypes.Add(new LeaveType { Id = 3, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        await _db.SaveChangesAsync();

        var request = new CreateLeaveRequestRequest
        {
            LeaveTypeId = 3,
            StartDate = new DateTime(2026, 7, 24),
            EndDate = new DateTime(2026, 7, 30)
        };

        // When
        var result = await _service.CreateAsync(3, request);

        // Then
        Assert.Equal(CreateLeaveRequestStatus.InsufficientBalance, result.Status);
        Assert.Equal(5, result.RequestedDays);
        Assert.Equal(2, result.RemainingDays);
    }

    [Fact]
    public async Task CreateAsync_ReturnsOverlap()
    {
        // Given
        _db.Users.Add(new User { Id = 4, Email = "___", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 4, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 4,
            EmployeeId = 4,
            LeaveTypeId = 4,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateTime(2026, 7, 27),
            EndDate = new DateTime(2026, 7, 31),
        });
        await _db.SaveChangesAsync();

        var request = new CreateLeaveRequestRequest
        {
            LeaveTypeId = 4,
            StartDate = new DateTime(2026, 7, 29),   // upada u 27–31
            EndDate = new DateTime(2026, 8, 3),
        };

        // When
        var result = await _service.CreateAsync(4, request);

        // Then
        Assert.Equal(CreateLeaveRequestStatus.Overlap, result.Status);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreated()
    {
        // Given
        _db.Users.Add(new User { Id = 5, Email = "___", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 5, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        await _db.SaveChangesAsync();

        var request = new CreateLeaveRequestRequest
        {
            LeaveTypeId = 5,
            StartDate = new DateTime(2026, 7, 29),
            EndDate = new DateTime(2026, 8, 3),
        };

        // When
        var result = await _service.CreateAsync(5, request);

        // Then
        Assert.Equal(CreateLeaveRequestStatus.Created, result.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsRequest_WhenEmployeeOwnsIt()
    {
        // Given
        _db.Users.Add(new User { Id = 10, Email = "owner@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 10, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 10,
            EmployeeId = 10,
            LeaveTypeId = 10,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateTime(2026, 7, 27),
            EndDate = new DateTime(2026, 7, 31),
        });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetByIdAsync(10, requestingUserId: 10, role: UserRole.Employee);

        // Then
        Assert.NotNull(result);
        Assert.Equal(10, result!.EmployeeId);
        Assert.Equal(5, result.WorkingDays);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenOtherEmployeeRequestsIt()
    {
        // Given
        _db.Users.Add(new User { Id = 10, Email = "owner@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 10, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 10,
            EmployeeId = 10,
            LeaveTypeId = 10,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateTime(2026, 7, 27),
            EndDate = new DateTime(2026, 7, 31),
        });
        await _db.SaveChangesAsync();

        // When — a different employee asks for someone else's request
        var result = await _service.GetByIdAsync(10, requestingUserId: 99, role: UserRole.Employee);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsRequest_WhenHrRequestsAnyone()
    {
        // Given
        _db.Users.Add(new User { Id = 10, Email = "owner@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 10, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 10,
            EmployeeId = 10,
            LeaveTypeId = 10,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateTime(2026, 7, 27),
            EndDate = new DateTime(2026, 7, 31),
        });
        await _db.SaveChangesAsync();

        // When — HR is not the owner but may see everything
        var result = await _service.GetByIdAsync(10, requestingUserId: 99, role: UserRole.HR);

        // Then
        Assert.NotNull(result);
        Assert.Equal(10, result!.EmployeeId);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // When
        var result = await _service.GetByIdAsync(999, requestingUserId: 1, role: UserRole.HR);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsOnePageAndTotalCount()
    {
        // Given
        _db.Users.Add(new User { Id = 10, Email = "ana@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 10, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.AddRange(
            NewRequest(1, employeeId: 10, leaveTypeId: 10, new DateTime(2026, 7, 6), new DateTime(2026, 7, 10)),
            NewRequest(2, employeeId: 10, leaveTypeId: 10, new DateTime(2026, 7, 13), new DateTime(2026, 7, 17)),
            NewRequest(3, employeeId: 10, leaveTypeId: 10, new DateTime(2026, 7, 20), new DateTime(2026, 7, 24)));
        await _db.SaveChangesAsync();

        // When — page 1 of size 2 over 3 rows
        var result = await _service.GetPagedAsync(new GetLeaveRequestsRequest { Page = 1, PageSize = 2 });

        // Then
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetPagedAsync_FiltersByStatus()
    {
        // Given
        _db.Users.Add(new User { Id = 10, Email = "ana@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 10, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.AddRange(
            NewRequest(1, 10, 10, new DateTime(2026, 7, 6), new DateTime(2026, 7, 10), LeaveRequestStatus.Pending),
            NewRequest(2, 10, 10, new DateTime(2026, 7, 13), new DateTime(2026, 7, 17), LeaveRequestStatus.Pending),
            NewRequest(3, 10, 10, new DateTime(2026, 7, 20), new DateTime(2026, 7, 24), LeaveRequestStatus.Approved));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetPagedAsync(new GetLeaveRequestsRequest { Status = LeaveRequestStatus.Approved });

        // Then
        Assert.Equal(1, result.TotalCount);
        Assert.Equal(LeaveRequestStatus.Approved, result.Items.Single().Status);
    }

    [Fact]
    public async Task GetMinePagedAsync_ReturnsOnlyCallersRequests()
    {
        // Given
        _db.Users.AddRange(
            new User { Id = 10, Email = "ana@vacayay.test", DaysOff = 20 },
            new User { Id = 20, Email = "marko@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 10, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.AddRange(
            NewRequest(1, employeeId: 10, leaveTypeId: 10, new DateTime(2026, 7, 6), new DateTime(2026, 7, 10)),
            NewRequest(2, employeeId: 10, leaveTypeId: 10, new DateTime(2026, 7, 13), new DateTime(2026, 7, 17)),
            NewRequest(3, employeeId: 20, leaveTypeId: 10, new DateTime(2026, 7, 20), new DateTime(2026, 7, 24)));
        await _db.SaveChangesAsync();

        // When — caller 10, but the request tries to page employee 20's rows
        var result = await _service.GetMinePagedAsync(10, new GetLeaveRequestsRequest { EmployeeId = 20 });

        // Then — the caller's id wins; only employee 10's rows come back
        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, r => Assert.Equal(10, r.EmployeeId));
    }

    [Fact]
    public async Task GetSummaryAsync_AggregatesCountsAndWorkingDays()
    {
        // Given
        _db.Users.Add(new User { Id = 10, Email = "ana@vacayay.test", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 10, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.AddRange(
            NewRequest(1, 10, 10, new DateTime(2026, 7, 6), new DateTime(2026, 7, 10), LeaveRequestStatus.Pending),
            NewRequest(2, 10, 10, new DateTime(2026, 7, 13), new DateTime(2026, 7, 17), LeaveRequestStatus.Pending),
            NewRequest(3, 10, 10, new DateTime(2026, 7, 20), new DateTime(2026, 7, 24), LeaveRequestStatus.Approved),
            NewRequest(4, 10, 10, new DateTime(2026, 7, 27), new DateTime(2026, 7, 31), LeaveRequestStatus.Rejected));
        await _db.SaveChangesAsync();

        // When
        var summary = await _service.GetSummaryAsync();

        // Then
        Assert.Equal(4, summary.TotalCount);
        Assert.Equal(2, summary.CountByStatus["Pending"]);
        Assert.Equal(1, summary.CountByStatus["Approved"]);
        Assert.Equal(1, summary.CountByStatus["Rejected"]);

        // Only Pending + Approved count toward days-per-type (3 requests × 5 working days).
        Assert.Equal(15, summary.DaysByType.Single(t => t.LeaveTypeName == LeaveTypeName.Annual).WorkingDays);
    }

    private static LeaveRequest NewRequest(
        int id, int employeeId, int leaveTypeId, DateTime start, DateTime end,
        LeaveRequestStatus status = LeaveRequestStatus.Pending) => new()
    {
        Id = id,
        EmployeeId = employeeId,
        LeaveTypeId = leaveTypeId,
        Status = status,
        StartDate = start,
        EndDate = end,
    };

    // Unique database name per call so tests never share state.
    private static VacaYAYDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<VacaYAYDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new VacaYAYDbContext(options);
    }
}
