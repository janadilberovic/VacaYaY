using Mapster;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.Employee;
using VacaYAY.Business.Services.Employee;
using VacaYAY.Data;
using UserEntity = VacaYAY.Domain.Entities.User;

namespace VacaYAY.UnitTests;

public class EmployeeServiceTests : IDisposable
{
    private readonly VacaYAYDbContext _db;
    private readonly EmployeeService _service;

    // EmployeeService maps with Mapster's global config, populated at app start by a scan.
    // Register it once here so .Adapt/.ProjectToType behave the same under test.
    static EmployeeServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(EmployeeService).Assembly);
    }

    public EmployeeServiceTests()
    {
        _db = NewDb();
        _service = new EmployeeService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetPagedAsync_ReturnsActiveEmployeesOrderedByName()
    {
        // Given
        _db.Users.AddRange(
            new UserEntity { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test" },
            new UserEntity { Id = 2, FirstName = "Marko", LastName = "Aric", Email = "marko@vacayay.test" });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetPagedAsync(new GetEmployeesRequest());

        // Then
        Assert.Equal(2, result.TotalCount);
        Assert.Equal("Aric", result.Items[0].LastName);
        Assert.Equal("Zoric", result.Items[1].LastName);
    }

    [Fact]
    public async Task GetPagedAsync_ExcludesArchivedEmployees_ByDefault()
    {
        // Given
        _db.Users.AddRange(
            new UserEntity { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test" },
            new UserEntity { Id = 2, FirstName = "Marko", LastName = "Aric", Email = "marko@vacayay.test", IsDeleted = true });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetPagedAsync(new GetEmployeesRequest());

        // Then — the global query filter hides the archived account
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("ana@vacayay.test", Assert.Single(result.Items).Email);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsOnlyArchivedEmployees_WhenArchivedRequested()
    {
        // Given
        _db.Users.AddRange(
            new UserEntity { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test" },
            new UserEntity { Id = 2, FirstName = "Marko", LastName = "Aric", Email = "marko@vacayay.test", IsDeleted = true });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetPagedAsync(new GetEmployeesRequest { Archived = true });

        // Then
        Assert.Equal(1, result.TotalCount);
        Assert.Equal("marko@vacayay.test", Assert.Single(result.Items).Email);
    }

    [Fact]
    public async Task GetPagedAsync_ReturnsOnePageAndTotalCount()
    {
        // Given
        _db.Users.AddRange(
            new UserEntity { Id = 1, FirstName = "A", LastName = "Aa", Email = "a@vacayay.test" },
            new UserEntity { Id = 2, FirstName = "B", LastName = "Bb", Email = "b@vacayay.test" },
            new UserEntity { Id = 3, FirstName = "C", LastName = "Cc", Email = "c@vacayay.test" });
        await _db.SaveChangesAsync();

        // When — page 1 of size 2 over 3 rows
        var result = await _service.GetPagedAsync(new GetEmployeesRequest { Page = 1, PageSize = 2 });

        // Then
        Assert.Equal(3, result.TotalCount);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsEmployee_WhenFound()
    {
        // Given
        _db.Users.Add(new UserEntity
        {
            Id = 1,
            FirstName = "Ana",
            LastName = "Zoric",
            Email = "ana@vacayay.test",
            Role = UserRole.HR,
            DaysOff = 20,
        });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetByIdAsync(1);

        // Then
        Assert.NotNull(result);
        Assert.Equal("ana@vacayay.test", result!.Email);
        Assert.Equal(UserRole.HR, result.Role);
        Assert.Equal(20, result.DaysOff);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        // When
        var result = await _service.GetByIdAsync(999);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenArchived()
    {
        // Given
        _db.Users.Add(new UserEntity { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test", IsDeleted = true });
        await _db.SaveChangesAsync();

        // When — the soft-delete filter hides it from a normal read
        var result = await _service.GetByIdAsync(1);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task GetMeAsync_ReturnsCallersProfile()
    {
        // Given
        _db.Users.Add(new UserEntity { Id = 7, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test" });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetMeAsync(7);

        // Then
        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        Assert.Equal("ana@vacayay.test", result.Email);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreated_WithTempPassword()
    {
        // Given
        var request = new CreateEmployeeRequest
        {
            FirstName = "Ana",
            LastName = "Zoric",
            Email = "ana@vacayay.test",
            Role = UserRole.Employee,
            DaysOff = 20,
        };

        // When
        var result = await _service.CreateAsync(request);

        // Then
        Assert.Equal(CreateEmployeeStatus.Created, result.Status);
        Assert.NotNull(result.Dto);
        Assert.Equal("ana@vacayay.test", result.Dto!.Email);
        Assert.False(string.IsNullOrWhiteSpace(result.TempPassword));

        // The account is provisioned into the first-login flow: temp password set, no real hash yet.
        var saved = await _db.Users.SingleAsync(u => u.Email == "ana@vacayay.test");
        Assert.True(saved.IsActive);
        Assert.Null(saved.PasswordHash);
        Assert.Equal(result.TempPassword, saved.TempPassword);
    }

    [Fact]
    public async Task CreateAsync_ReturnsEmailConflict_WhenActiveEmailExists()
    {
        // Given
        _db.Users.Add(new UserEntity { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test" });
        await _db.SaveChangesAsync();

        var request = new CreateEmployeeRequest { FirstName = "Other", LastName = "Person", Email = "ana@vacayay.test" };

        // When
        var result = await _service.CreateAsync(request);

        // Then
        Assert.Equal(CreateEmployeeStatus.EmailConflict, result.Status);
        Assert.Null(result.Dto);
    }

    [Fact]
    public async Task CreateAsync_ReturnsArchivedExists_WhenSoftDeletedEmailExists()
    {
        // Given
        _db.Users.Add(new UserEntity { Id = 7, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test", IsDeleted = true });
        await _db.SaveChangesAsync();

        var request = new CreateEmployeeRequest { FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test" };

        // When
        var result = await _service.CreateAsync(request);

        // Then — the email is held by an archived account; HR can restore it instead
        Assert.Equal(CreateEmployeeStatus.ArchivedExists, result.Status);
        Assert.Equal(7, result.ArchivedId);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesEditableFields()
    {
        // Given
        _db.Users.Add(new UserEntity
        {
            Id = 1,
            FirstName = "Ana",
            LastName = "Zoric",
            Email = "ana@vacayay.test",
            Department = "Old",
            JobTitle = "Junior",
            DaysOff = 20,
            IsActive = true,
        });
        await _db.SaveChangesAsync();

        var request = new UpdateEmployeeRequest
        {
            FirstName = "Ana",
            LastName = "Zoric",
            Department = "Engineering",
            JobTitle = "Senior",
            DaysOff = 25,
            IsActive = true,
        };

        // When
        var result = await _service.UpdateAsync(1, request);

        // Then
        Assert.NotNull(result);
        Assert.Equal("Engineering", result!.Department);
        Assert.Equal("Senior", result.JobTitle);
        Assert.Equal(25, result.DaysOff);
    }

    [Fact]
    public async Task UpdateAsync_LeavesEmailAndRoleUnchanged()
    {
        // Given
        _db.Users.Add(new UserEntity
        {
            Id = 1,
            FirstName = "Ana",
            LastName = "Zoric",
            Email = "ana@vacayay.test",
            Role = UserRole.HR,
        });
        await _db.SaveChangesAsync();

        var request = new UpdateEmployeeRequest { FirstName = "Ana", LastName = "Zoric", DaysOff = 10 };

        // When
        var result = await _service.UpdateAsync(1, request);

        // Then — Email and Role are immutable (ignored by the mapping config)
        Assert.NotNull(result);
        Assert.Equal("ana@vacayay.test", result!.Email);
        Assert.Equal(UserRole.HR, result.Role);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotFound()
    {
        // When
        var result = await _service.UpdateAsync(999, new UpdateEmployeeRequest());

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task ResetPasswordAsync_MintsNewTempPassword_AndClearsHash()
    {
        // Given
        _db.Users.Add(new UserEntity
        {
            Id = 1,
            FirstName = "Ana",
            LastName = "Zoric",
            Email = "ana@vacayay.test",
            PasswordHash = "existing-hash",
        });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ResetPasswordAsync(1);

        // Then
        Assert.NotNull(result);
        Assert.Equal(1, result!.EmployeeId);
        Assert.False(string.IsNullOrWhiteSpace(result.TempPassword));

        // Account is dropped back into the first-login flow.
        var saved = await _db.Users.SingleAsync(u => u.Id == 1);
        Assert.Null(saved.PasswordHash);
        Assert.Equal(result.TempPassword, saved.TempPassword);
    }

    [Fact]
    public async Task ResetPasswordAsync_ReturnsNull_WhenNotFound()
    {
        // When
        var result = await _service.ResetPasswordAsync(999);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_SoftDeletesEmployee_AndReturnsTrue()
    {
        // Given
        _db.Users.Add(new UserEntity { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test", IsActive = true });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.DeleteAsync(1);

        // Then
        Assert.True(result);
        var archived = await _db.Users.IgnoreQueryFilters().SingleAsync(u => u.Id == 1);
        Assert.True(archived.IsDeleted);
        Assert.False(archived.IsActive);
        Assert.NotNull(archived.DeletedAt);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotFound()
    {
        // When
        var result = await _service.DeleteAsync(999);

        // Then
        Assert.False(result);
    }

    [Fact]
    public async Task RestoreAsync_RestoresArchivedEmployee()
    {
        // Given
        _db.Users.Add(new UserEntity
        {
            Id = 1,
            FirstName = "Ana",
            LastName = "Zoric",
            Email = "ana@vacayay.test",
            IsDeleted = true,
            DeletedAt = new DateTime(2026, 1, 1),
            IsActive = false,
        });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.RestoreAsync(1);

        // Then
        Assert.NotNull(result);
        Assert.Equal("ana@vacayay.test", result!.Email);
        var restored = await _db.Users.SingleAsync(u => u.Id == 1);
        Assert.False(restored.IsDeleted);
        Assert.Null(restored.DeletedAt);
        Assert.True(restored.IsActive);
    }

    [Fact]
    public async Task RestoreAsync_ReturnsNull_WhenEmployeeIsNotArchived()
    {
        // Given
        _db.Users.Add(new UserEntity { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@vacayay.test" });
        await _db.SaveChangesAsync();

        // When — the account exists but isn't soft-deleted, so there's nothing to restore
        var result = await _service.RestoreAsync(1);

        // Then
        Assert.Null(result);
    }

    [Fact]
    public async Task RestoreAsync_ReturnsNull_WhenNotFound()
    {
        // When
        var result = await _service.RestoreAsync(999);

        // Then
        Assert.Null(result);
    }

    // Unique database name per call so tests never share state.
    private static VacaYAYDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<VacaYAYDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new VacaYAYDbContext(options);
    }
}
