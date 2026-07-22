namespace VacaYAY.Business.DTOs.EmployeeImport;

/// <summary>The legacy employees HR ticked in the import picker.</summary>
public class ImportLegacyEmployeesRequest
{
    public List<int> LegacyIds { get; set; } = new();
}
