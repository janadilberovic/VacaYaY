using Mapster;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.EmployeeImport;
using VacaYAY.Business.Services.Employee;
using VacaYAY.Business.Services.EmployeeImport;
using VacaYAY.Business.Validators.Employee;
using VacaYAY.Data;
using VacaYAY.Domain.Entities;

namespace VacaYAY.UnitTests;

public class EmployeeImportServiceTests : IDisposable
{
    private readonly VacaYAYDbContext _db;
    private readonly EmployeeImportService _service;

    // The import path maps legacy rows with Mapster's global config; register it once, as the app does.
    static EmployeeImportServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(EmployeeService).Assembly);
    }

    public EmployeeImportServiceTests()
    {
        _db = NewDb();
        _service = new EmployeeImportService(
            new LegacyEmployeeService(_db),
            new EmployeeService(_db),
            new CreateEmployeeRequestValidator(),
            _db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetRosterAsync_FlagsAlreadyImported_AndOrdersByName()
    {
        // Given
        _db.LegacyEmployees.AddRange(
            NewLegacy(1, "Marko", "Aric", "marko@old.test"),
            NewLegacy(2, "Ana", "Zoric", "ana@old.test"));
        // Marko already has a VacaYAY account under the same email.
        _db.Users.Add(new User { Id = 1, FirstName = "Marko", LastName = "Aric", Email = "marko@old.test" });
        await _db.SaveChangesAsync();

        // When
        var roster = await _service.GetRosterAsync();

        // Then — ordered by last name; Marko flagged, Ana not
        Assert.Equal(2, roster.Count);
        Assert.Equal("Aric", roster[0].LastName);
        Assert.True(roster[0].AlreadyImported);
        Assert.Equal("Zoric", roster[1].LastName);
        Assert.False(roster[1].AlreadyImported);
    }

    [Fact]
    public async Task GetRosterAsync_FlagsArchivedAccountsAsImported()
    {
        // Given — the account was archived, but its email is still taken (unique index spans soft-deletes)
        _db.LegacyEmployees.Add(NewLegacy(1, "Ana", "Zoric", "ana@old.test"));
        _db.Users.Add(new User { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@old.test", IsDeleted = true });
        await _db.SaveChangesAsync();

        // When
        var roster = await _service.GetRosterAsync();

        // Then
        Assert.True(Assert.Single(roster).AlreadyImported);
    }

    [Fact]
    public async Task ImportAsync_CreatesAccountsForSelectedLegacyEmployees()
    {
        // Given
        _db.LegacyEmployees.AddRange(
            NewLegacy(1, "Ana", "Zoric", "ana@old.test", title: "Developer", daysOff: 5),
            NewLegacy(2, "Marko", "Aric", "marko@old.test"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ImportAsync(new ImportLegacyEmployeesRequest { LegacyIds = new() { 1, 2 } });

        // Then
        Assert.Equal(2, result.Imported);
        Assert.Equal(0, result.Skipped);
        Assert.Equal(2, result.ImportedEmployees.Count);
        Assert.Equal(2, await _db.Users.CountAsync());

        // Field mapping: legacy Title → JobTitle, and the legacy balance is dropped for the standard 20.
        var ana = result.ImportedEmployees.Single(e => e.Email == "ana@old.test");
        Assert.Equal("Developer", ana.JobTitle);
        Assert.Equal(20, ana.DaysOff);
        Assert.Equal(UserRole.Employee, ana.Role);
    }

    [Fact]
    public async Task ImportAsync_SkipsEmployeesThatAlreadyHaveAccounts()
    {
        // Given
        _db.LegacyEmployees.Add(NewLegacy(1, "Ana", "Zoric", "ana@old.test"));
        _db.Users.Add(new User { Id = 1, FirstName = "Ana", LastName = "Zoric", Email = "ana@old.test" });
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ImportAsync(new ImportLegacyEmployeesRequest { LegacyIds = new() { 1 } });

        // Then — CreateAsync reports the email conflict, so the import counts it as skipped
        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Skipped);
    }

    [Fact]
    public async Task ImportAsync_CountsSelectedIdsTheLegacySystemNoLongerReturns()
    {
        // Given
        _db.LegacyEmployees.Add(NewLegacy(1, "Ana", "Zoric", "ana@old.test"));
        await _db.SaveChangesAsync();

        // When — id 99 isn't in the legacy roster
        var result = await _service.ImportAsync(new ImportLegacyEmployeesRequest { LegacyIds = new() { 1, 99 } });

        // Then
        Assert.Equal(1, result.Imported);
        Assert.Equal(1, result.NotFound);
    }

    [Fact]
    public async Task ImportAsync_CountsLegacyRowsThatFailValidation()
    {
        // Given — a legacy row with a malformed email must not become a User via the import path
        _db.LegacyEmployees.Add(NewLegacy(1, "Ana", "Zoric", "not-an-email"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ImportAsync(new ImportLegacyEmployeesRequest { LegacyIds = new() { 1 } });

        // Then
        Assert.Equal(0, result.Imported);
        Assert.Equal(1, result.Invalid);
        Assert.Equal(0, await _db.Users.CountAsync());
    }

    [Fact]
    public async Task ImportAsync_ImportsDuplicateSelectedIdOnlyOnce()
    {
        // Given
        _db.LegacyEmployees.Add(NewLegacy(1, "Ana", "Zoric", "ana@old.test"));
        await _db.SaveChangesAsync();

        // When — the same id is selected twice
        var result = await _service.ImportAsync(new ImportLegacyEmployeesRequest { LegacyIds = new() { 1, 1 } });

        // Then — Distinct() collapses it to a single import
        Assert.Equal(1, result.Imported);
        Assert.Equal(1, await _db.Users.CountAsync());
    }

    [Fact]
    public async Task ImportAsync_ReturnsRefreshedRosterWithNewlyImportedFlagged()
    {
        // Given
        _db.LegacyEmployees.Add(NewLegacy(1, "Ana", "Zoric", "ana@old.test"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.ImportAsync(new ImportLegacyEmployeesRequest { LegacyIds = new() { 1 } });

        // Then — the roster returned alongside the result shows the just-imported row as taken
        var rosterItem = Assert.Single(result.Roster);
        Assert.True(rosterItem.AlreadyImported);
    }

    private static LegacyEmployee NewLegacy(
        int legacyId, string firstName, string lastName, string email,
        string? title = null, int daysOff = 0) => new()
    {
        LegacyId = legacyId,
        FirstName = firstName,
        LastName = lastName,
        Email = email,
        Title = title,
        DaysOff = daysOff,
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
