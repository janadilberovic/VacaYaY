using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VacaYAY.Domain.Entities;

/// <summary>
/// A leave request submitted by an employee. Flows Pending → Approved | Rejected (optionally Cancelled).
/// The working-day count is subtracted from the employee's balance on approval.
/// </summary>
public class LeaveRequest
{
    public int Id { get; set; }

    [Required]
    public int EmployeeId { get; set; }

    [Required]
    public int LeaveTypeId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }

    /// <summary>Optional comment provided by the employee.</summary>
    public string? Reason { get; set; }

    public LeaveRequestStatus Status { get; set; } = LeaveRequestStatus.Pending;

    public DateTime CreatedAt { get; set; }

    // Review fields — filled in when HR decides on the request.
    public string? HrComment { get; set; }

    public string? HrName { get; set; }

    public DateTime? ReviewedAt { get; set; }

    // Navigation
    [ForeignKey(nameof(EmployeeId))]
    public User? Employee { get; set; }

    [ForeignKey(nameof(LeaveTypeId))]
    public LeaveType? LeaveType { get; set; }
}


