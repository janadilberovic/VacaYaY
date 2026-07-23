using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.Services.LeaveRequest;
using VacaYAY.Data;
using VacaYAY.Domain.Entities;
using VacaYAY.Business.DTOs.LeaveRequest;

namespace VacaYAY.IntegrationTests;

public class LeaveRequestServiceIntegrationTests : IDisposable
{
    private const string ConnString =
        "Server=localhost;Port=3307;Database=vacayay_test;User=root;Password=test1234;";

    private readonly VacaYAYDbContext _db;
    private readonly LeaveRequestService _service;


    public LeaveRequestServiceIntegrationTests()
    {
        _db = NewDb();
        _service = new LeaveRequestService(_db, new SerbianHolidayProvider());
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task CanConnectToDatabase()
    {
        Assert.True(await _db.Database.CanConnectAsync());
    }

    private static DbContextOptions<VacaYAYDbContext> BuildOptions() =>
        new DbContextOptionsBuilder<VacaYAYDbContext>()
            .UseMySql(ConnString, ServerVersion.AutoDetect(ConnString))
            .Options;


    private static VacaYAYDbContext NewDb()
    {
        var db = new VacaYAYDbContext(BuildOptions());
        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        return db;
    }

    [Fact]
    public async Task ApproveAsync_SetsStatusToApproved_OnPendingRequest()
    {
        // Given
        _db.Users.Add(new User { Id = 1, Email = "new@vacayay.com", DaysOff = 20 });
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
        var result = await _service.ApproveAsync(1, hrUserId: 100, new ReviewLeaveRequestRequest { HrComment = "approved" });
        // Then
        Assert.Equal(ReviewLeaveRequestStatus.Reviewed, result.Status);
        //side effect da pribavim sveze podatke zbog ExecuteUpdateAsync
        using var verify = OpenDb();
        var status = await verify.LeaveRequests.Where(r => r.Id == 1).Select(r => r.Status).SingleAsync();
        Assert.Equal(LeaveRequestStatus.Approved, status);
        var daysOff = await verify.Users.Where(u => u.Id == 1)
        .Select(u => u.DaysOff).SingleAsync();
        Assert.Equal(15, daysOff);
    }
    [Fact]
    public async Task ApproveAsync_ReturnsInsufficientBalance_WhenDaysOffTooLow()
    {
        // Given
        _db.Users.Add(new User { Id = 1, Email = "new@vacayay.com", DaysOff = 2 });
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
        var result = await _service.ApproveAsync(1, hrUserId: 100, new ReviewLeaveRequestRequest { HrComment = "Insufficient Balance" });
        // Then
        Assert.Equal(ReviewLeaveRequestStatus.InsufficientBalance, result.Status);
        using var verify = OpenDb();
        var status = await verify.LeaveRequests.Where(r => r.Id == 1).Select(r => r.Status).SingleAsync();
        Assert.Equal(LeaveRequestStatus.Pending, status);
        var daysOff = await verify.Users.Where(u => u.Id == 1)
        .Select(u => u.DaysOff).SingleAsync();
        Assert.Equal(2, daysOff);
    }
    [Fact]
    public async Task ApproveAsync_ReturnsNotPending_WhenAlreadyApproved()
    {
        // Given
        _db.Users.Add(new User { Id = 1, Email = "new@vacayay.com", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 1, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            Status = LeaveRequestStatus.Approved,
            StartDate = new DateTime(2026, 7, 27),
            EndDate = new DateTime(2026, 7, 31),
        });
        await _db.SaveChangesAsync();
        // When
        var result = await _service.ApproveAsync(1, hrUserId: 100, new ReviewLeaveRequestRequest { HrComment = "approved" });
        // Then
        Assert.Equal(ReviewLeaveRequestStatus.NotPending, result.Status);
        //side effect da pribavim sveze podatke zbog ExecuteUpdateAsync
        using var verify = OpenDb();
        var status = await verify.LeaveRequests.Where(r => r.Id == 1).Select(r => r.Status).SingleAsync();
        Assert.Equal(LeaveRequestStatus.Approved, status);
        var daysOff = await verify.Users.Where(u => u.Id == 1)
        .Select(u => u.DaysOff).SingleAsync();
        Assert.Equal(20, daysOff);
    }
     [Fact]
    public async Task ApproveAsync_KeepsBalanceUnchanged_WhenTypeIsNonCounting()
    {
        // Given
        _db.Users.Add(new User { Id = 1, Email = "new@vacayay.com", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 1, Name = LeaveTypeName.Annual, CountsAgainstBalance = false });
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
        var result = await _service.ApproveAsync(1, hrUserId: 100, new ReviewLeaveRequestRequest { HrComment = "approved" });
        // Then
        Assert.Equal(ReviewLeaveRequestStatus.Reviewed, result.Status);
        //side effect da pribavim sveze podatke zbog ExecuteUpdateAsync
        using var verify = OpenDb();
        var status = await verify.LeaveRequests.Where(r => r.Id == 1).Select(r => r.Status).SingleAsync();
        Assert.Equal(LeaveRequestStatus.Approved, status);
        var daysOff = await verify.Users.Where(u => u.Id == 1)
        .Select(u => u.DaysOff).SingleAsync();
        Assert.Equal(20, daysOff);
    }

    [Fact]
    public async Task ApproveAsync_ReturnsNotFound_WhenRequestDoesNotExist()
    {
        // Given — nothing seeded

        // When
        var result = await _service.ApproveAsync(999, hrUserId: 100, new ReviewLeaveRequestRequest());

        // Then
        Assert.Equal(ReviewLeaveRequestStatus.NotFound, result.Status);
    }
    [Fact]
    public async Task CancelAsync_ReturnsCancelled_OnApprovedRequest()
    {
        // Given
        _db.Users.Add(new User { Id = 1, Email = "new@vacayay.com", DaysOff = 10 });
        _db.LeaveTypes.Add(new LeaveType { Id = 1, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            Status = LeaveRequestStatus.Approved,
            StartDate = new DateTime(2026, 11, 10),
            EndDate = new DateTime(2026, 11, 15),
        });
        await _db.SaveChangesAsync();
        // When
        var result= await _service.CancelAsync(1,1);

        // Then
        Assert.Equal(ReviewLeaveRequestStatus.Reviewed, result.Status);
        using var verify = OpenDb();
        var daysOff= await verify.Users.Where(u=> u.Id==1).Select(u=> u.DaysOff).SingleAsync();
        Assert.Equal(13,daysOff);
    }

    [Fact]
    public async Task CancelAsync_KeepsBalanceUnchanged_WhenCancellingPendingRequest()
    {
        // Given — a pending request never deducted anything, so cancelling refunds nothing
        _db.Users.Add(new User { Id = 1, Email = "new@vacayay.com", DaysOff = 20 });
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
        var result = await _service.CancelAsync(1, employeeId: 1);

        // Then
        Assert.Equal(ReviewLeaveRequestStatus.Reviewed, result.Status);

        using var verify = OpenDb();
        var status = await verify.LeaveRequests.Where(r => r.Id == 1).Select(r => r.Status).SingleAsync();
        Assert.Equal(LeaveRequestStatus.Cancelled, status);
        var daysOff = await verify.Users.Where(u => u.Id == 1).Select(u => u.DaysOff).SingleAsync();
        Assert.Equal(20, daysOff);
    }

    [Fact]
    public async Task CancelAsync_ReturnsNotFound_WhenCallerIsNotOwner()
    {
        // Given — request owned by employee 1
        _db.Users.Add(new User { Id = 1, Email = "new@vacayay.com", DaysOff = 20 });
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

        // When — a different employee tries to cancel it
        var result = await _service.CancelAsync(1, employeeId: 99);

        // Then — other people's requests are hidden as "not found"
        Assert.Equal(ReviewLeaveRequestStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task CancelAsync_ReturnsNotPending_WhenRequestAlreadyRejected()
    {
        // Given
        _db.Users.Add(new User { Id = 1, Email = "new@vacayay.com", DaysOff = 20 });
        _db.LeaveTypes.Add(new LeaveType { Id = 1, Name = LeaveTypeName.Annual, CountsAgainstBalance = true });
        _db.LeaveRequests.Add(new LeaveRequest
        {
            Id = 1,
            EmployeeId = 1,
            LeaveTypeId = 1,
            Status = LeaveRequestStatus.Rejected,
            StartDate = new DateTime(2026, 7, 27),
            EndDate = new DateTime(2026, 7, 31),
        });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.CancelAsync(1, employeeId: 1);

        // Then
        Assert.Equal(ReviewLeaveRequestStatus.NotPending, result.Status);
    }

    private static VacaYAYDbContext OpenDb() => new(BuildOptions());
}
