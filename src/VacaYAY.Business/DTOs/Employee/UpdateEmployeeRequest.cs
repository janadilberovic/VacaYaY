namespace VacaYAY.Business.DTOs.Employee;

public class UpdateEmployeeRequest
{


    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string? Department { get; set; }

    public string? JobTitle { get; set; }

    public DateTime? EmploymentStartDate { get; set; }

    public DateTime? EmploymentEndDate { get; set; }

    public int DaysOff { get; set; }

    public bool IsActive { get; set; }
}
