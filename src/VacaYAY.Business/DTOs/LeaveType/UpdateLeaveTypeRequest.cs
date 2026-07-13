namespace VacaYAY.Business.DTOs.LeaveType;

public class UpdateLeaveTypeRequest
{
    // Name is the type's identity (a LeaveTypeName category) and is immutable after creation.
    // HR edits the fields below; to change the category, create a new leave type.

    public LeaveColor? Color { get; set; }

    public bool IsPaid { get; set; }

    public bool CountsAgainstBalance { get; set; }
}
