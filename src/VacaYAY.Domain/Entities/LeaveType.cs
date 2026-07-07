namespace VacaYAY.Domain.Entities;

/// <summary>
/// A category of leave (Annual, Sick, Paid, Unpaid…) with minimal per-type rules.
/// </summary>
public class LeaveType
{
    public int Id { get; set; }

    public LeaveTypeName Name { get; set; }

    /// <summary>Optional color used to render the type in the UI (e.g. hex code).</summary>
    public string? Color { get; set; }

    public bool IsPaid { get; set; }

    /// <summary>Whether approved requests of this type reduce the employee's day balance.</summary>
    public bool CountsAgainstBalance { get; set; }

    // Navigation
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}

