using VacaYAY.Business.DTOs.Employee;

namespace VacaYAY.Business.DTOs.EmployeeImport;

/// <summary>
/// Outcome of an import. <see cref="Roster"/> carries the refreshed picker list so the client does
/// not need a follow-up request. Temp passwords are never included — HR issues them via reset.
/// </summary>
public class ImportLegacyEmployeesResult
{
    public int Imported { get; set; }

    /// <summary>Selected employees that already had a VacaYAY account (active or archived).</summary>
    public int Skipped { get; set; }

    /// <summary>Selected ids the old system no longer returns.</summary>
    public int NotFound { get; set; }

    /// <summary>Selected rows whose legacy data failed validation and were left behind.</summary>
    public int Invalid { get; set; }

    public List<EmployeeDto> ImportedEmployees { get; set; } = new();

    public List<LegacyEmployeeRosterItemDto> Roster { get; set; } = new();
}
