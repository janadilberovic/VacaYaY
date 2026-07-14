namespace VacaYAY.Business.DTOs.Employee;


public class CreateEmployeeResponse
{
    public EmployeeDto Employee { get; set; } = null!;

    public string TempPassword { get; set; } = string.Empty;
}
