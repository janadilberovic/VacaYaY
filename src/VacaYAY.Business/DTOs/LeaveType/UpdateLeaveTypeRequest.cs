namespace VacaYAY.Business.DTOs.LeaveType;

public class UpdateLeaveTypeRequest
{
    public LeaveTypeName Name { get; set; }

    public LeaveColor? Color { get; set; }

    public bool IsPaid { get; set; }

    public bool CountsAgainstBalance { get; set; }
}
