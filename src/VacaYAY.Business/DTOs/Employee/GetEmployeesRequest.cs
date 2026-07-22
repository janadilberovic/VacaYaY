namespace VacaYAY.Business.DTOs.Employee;

public class GetEmployeesRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    /// <summary>Lists archived (soft-deleted) accounts instead of active ones.</summary>
    public bool Archived { get; set; }
}
