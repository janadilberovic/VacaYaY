namespace VacaYAY.Domain.Entities;

/// <summary>
/// A category of leave (Annual, Sick, Paid, Unpaid…) with minimal per-type rules.
/// </summary>
public class LeaveType
{
    public int Id { get; set; }

    public LeaveTypeName Name { get; set; }

    /// <summary>Optional color used to render the type in the UI; the client maps it to a hex value.</summary>
    public LeaveColor? Color { get; set; }

    public bool IsPaid { get; set; }

    /// <summary>Whether approved requests of this type reduce the employee's day balance.</summary>
    public bool CountsAgainstBalance { get; set; }

    /// <summary>Soft-delete flag; deleted types are hidden via a global query filter.</summary>
    public bool IsDeleted { get; set; }

    // Navigation
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}

