using System.ComponentModel.DataAnnotations;

namespace VacaYAY.Domain.Entities;

/// <summary>
/// Application user. Acts as both the login identity and the employee record.
/// No public registration — accounts are created by HR.
/// </summary>
public class User
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(256)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>Hashed login password (never store plaintext).</summary>
    public string? PasswordHash { get; set; }

    public UserRole Role { get; set; }

    public string? Department { get; set; }

    public string? JobTitle { get; set; }

    public DateTime? HireDate { get; set; }

    public DateTime? EmploymentStartDate { get; set; }

    /// <summary>Contract end date. Null for permanent employment.</summary>
    public DateTime? EmploymentEndDate { get; set; }

    /// <summary>Available leave balance (days off remaining).</summary>
    public int DaysOff { get; set; }

    /// <summary>Temporary password issued at account creation, until the user sets their own.</summary>
    public string? TempPassword { get; set; }

    public string? ProfileImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    // Soft delete: the user disappears from default lists but stays in history.
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }

    // Navigation
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
}

