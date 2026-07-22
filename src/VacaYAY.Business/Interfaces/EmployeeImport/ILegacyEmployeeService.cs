using VacaYAY.Business.DTOs.EmployeeImport;

namespace VacaYAY.Business.Interfaces.EmployeeImport;

/// <summary>Reads the old system's employee table. Read-only — the old system is never written to.</summary>
public interface ILegacyEmployeeService
{
    Task<IReadOnlyList<LegacyEmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);
}
