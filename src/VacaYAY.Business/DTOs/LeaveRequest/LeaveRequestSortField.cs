namespace VacaYAY.Business.DTOs.LeaveRequest;

public enum LeaveRequestSortField
{
    /// <summary>Pending first, then newest start date — the HR review order.</summary>
    Default = 0,
    StartDate = 1,
    EndDate = 2,
    CreatedAt = 3,
    EmployeeName = 4,
    Status = 5,
}
