namespace VacaYAY.Business.DTOs.LeaveRequest;

public class LeaveRequestDto
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public string EmployeeName { get; set; } = string.Empty;

    public int LeaveTypeId { get; set; }

    public LeaveTypeName LeaveTypeName { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    /// <summary>Working days in the range, excluding weekends and Serbian public holidays.</summary>
    public int WorkingDays { get; set; }

    public string? Reason { get; set; }

    public LeaveRequestStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public string? HrComment { get; set; }

    public string? HrName { get; set; }

    public DateTime? ReviewedAt { get; set; }
}
