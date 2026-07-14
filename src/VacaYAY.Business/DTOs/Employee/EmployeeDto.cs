namespace VacaYAY.Business.DTOs.Employee;
public class EmployeeDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string? Department { get; set; }

    public string? JobTitle { get; set; }

    public DateTime? HireDate { get; set; }

    public DateTime? EmploymentStartDate { get; set; }

    public DateTime? EmploymentEndDate { get; set; }

    public int DaysOff { get; set; }

    public string? ProfileImageUrl { get; set; }

    public bool IsActive { get; set; }
}
