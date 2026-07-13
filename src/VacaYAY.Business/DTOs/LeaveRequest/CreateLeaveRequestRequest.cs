namespace VacaYAY.Business.DTOs.LeaveRequest;

public class CreateLeaveRequestRequest
{
    public int LeaveTypeId { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public string? Reason { get; set; }
}
