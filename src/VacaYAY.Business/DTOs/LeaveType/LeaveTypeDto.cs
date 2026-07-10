namespace VacaYAY.Business.DTOs.LeaveType;

public class LeaveTypeDto
{
    public int Id { get; set; }

    public LeaveTypeName Name { get; set; }

    public LeaveColor? Color { get; set; }

    public bool IsPaid { get; set; }

    public bool CountsAgainstBalance { get; set; }
}
