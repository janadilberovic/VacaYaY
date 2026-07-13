namespace VacaYAY.Business.DTOs.LeaveType;

public class UpdateLeaveTypeRequest
{


    public LeaveColor? Color { get; set; }

    public bool IsPaid { get; set; }

    public bool CountsAgainstBalance { get; set; }
}
