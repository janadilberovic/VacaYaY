using VacaYAY.Business.DTOs.EmployeeImport;

namespace VacaYAY.Business.Interfaces.EmployeeImport;

public interface IEmployeeImportService
{
    /// <summary>The old system's roster, each row flagged if it already has a VacaYAY account.</summary>
    Task<IReadOnlyList<LegacyEmployeeRosterItemDto>> GetRosterAsync(CancellationToken cancellationToken = default);

    /// <summary>Creates VacaYAY accounts for the selected legacy employees.</summary>
    Task<ImportLegacyEmployeesResult> ImportAsync(ImportLegacyEmployeesRequest request, CancellationToken cancellationToken = default);
}
