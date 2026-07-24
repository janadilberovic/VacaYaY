using Mapster;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.Services.EmployeeImport;
using VacaYAY.Data;
using VacaYAY.Domain.Entities;

namespace VacaYAY.UnitTests;

public class LegacyEmployeeServiceTests : IDisposable
{
    private readonly VacaYAYDbContext _db;
    private readonly LegacyEmployeeService _service;

    // GetAllAsync maps rows with Mapster's global config; register it once, as the app does.
    static LegacyEmployeeServiceTests()
    {
        TypeAdapterConfig.GlobalSettings.Scan(typeof(LegacyEmployeeService).Assembly);
    }

    public LegacyEmployeeServiceTests()
    {
        _db = NewDb();
        _service = new LegacyEmployeeService(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task GetAllAsync_ReturnsEmpty_WhenNoLegacyEmployees()
    {
        Assert.Empty(await _service.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_OrdersByLegacyId()
    {
        // Given — inserted out of order
        _db.LegacyEmployees.AddRange(
            NewLegacy(3, "Ana", "Zoric", "ana@old.test"),
            NewLegacy(1, "Marko", "Aric", "marko@old.test"),
            NewLegacy(2, "Ivan", "Balic", "ivan@old.test"));
        await _db.SaveChangesAsync();

        // When
        var result = await _service.GetAllAsync();

        // Then
        Assert.Equal(new[] { 1, 2, 3 }, result.Select(e => e.LegacyId));
    }

    [Fact]
    public async Task GetAllAsync_MapsLegacyFieldsAsIs()
    {
        // Given
        _db.LegacyEmployees.Add(new LegacyEmployee
        {
            LegacyId = 1,
            FirstName = "Ana",
            LastName = "Zoric",
            Email = "ana@old.test",
            Department = "Engineering",
            Title = "Developer",
            HiredOn = new DateTime(2020, 1, 15),
            ContractEnd = new DateTime(2026, 1, 15),
            DaysOff = 5,
        });
        await _db.SaveChangesAsync();

        // When
        var dto = Assert.Single(await _service.GetAllAsync());

        // Then — the old system's field names and values pass through untouched
        Assert.Equal(1, dto.LegacyId);
        Assert.Equal("Ana", dto.FirstName);
        Assert.Equal("Zoric", dto.LastName);
        Assert.Equal("ana@old.test", dto.Email);
        Assert.Equal("Engineering", dto.Department);
        Assert.Equal("Developer", dto.Title);
        Assert.Equal(new DateTime(2020, 1, 15), dto.HiredOn);
        Assert.Equal(new DateTime(2026, 1, 15), dto.ContractEnd);
        Assert.Equal(5, dto.DaysOff);
    }

    private static LegacyEmployee NewLegacy(int legacyId, string firstName, string lastName, string email) => new()
    {
        LegacyId = legacyId,
        FirstName = firstName,
        LastName = lastName,
        Email = email,
    };

    private static VacaYAYDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<VacaYAYDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new VacaYAYDbContext(options);
    }
}
