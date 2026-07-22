namespace VacaYAY.Business.DTOs.EmployeeImport;

/// <summary>An employee as the old system returns them. Field names are the old system's, not VacaYAY's.</summary>
public class LegacyEmployeeDto
{
    public int LegacyId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? Department { get; set; }

    public string? Title { get; set; }

    public DateTime? HiredOn { get; set; }

    public DateTime? ContractEnd { get; set; }

    public int DaysOff { get; set; }
}
