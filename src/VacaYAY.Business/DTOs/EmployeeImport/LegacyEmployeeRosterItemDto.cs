namespace VacaYAY.Business.DTOs.EmployeeImport;

/// <summary>
/// A legacy employee as shown in HR's import picker. <see cref="AlreadyImported"/> drives the
/// disabled checkbox, so HR can see who has been taken across without being able to re-import them.
/// </summary>
public class LegacyEmployeeRosterItemDto
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

    public bool AlreadyImported { get; set; }
}
