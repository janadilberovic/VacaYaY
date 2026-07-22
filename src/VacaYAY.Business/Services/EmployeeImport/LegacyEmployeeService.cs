using Mapster;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.EmployeeImport;
using VacaYAY.Business.Interfaces.EmployeeImport;
using VacaYAY.Data;

namespace VacaYAY.Business.Services.EmployeeImport;

/// <summary>
/// Serves the old system's employee table. Stands in for a third-party HR system, so it exposes
/// the legacy field names as-is and never writes.
/// </summary>
public class LegacyEmployeeService : ILegacyEmployeeService
{
    private readonly VacaYAYDbContext _db;

    public LegacyEmployeeService(VacaYAYDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LegacyEmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _db.LegacyEmployees
            .AsNoTracking()
            .OrderBy(e => e.LegacyId)
            .ToListAsync(cancellationToken);

        return employees.Adapt<List<LegacyEmployeeDto>>();
    }
}
