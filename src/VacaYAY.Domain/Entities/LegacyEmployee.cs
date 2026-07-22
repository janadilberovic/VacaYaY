using System.ComponentModel.DataAnnotations;

namespace VacaYAY.Domain.Entities;

/// <summary>
/// An employee record in the company's old HR system. Read-only reference data: the old system is
/// never written to, it only feeds the one-time migration into <see cref="User"/>.
/// </summary>
public class LegacyEmployee
{
    public int LegacyId { get; set; }

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

    public string? Department { get; set; }

    /// <summary>The old system's job title; maps to <see cref="User.JobTitle"/> on import.</summary>
    public string? Title { get; set; }

    public DateTime? HiredOn { get; set; }

    /// <summary>Contract end date. Null for permanent employment.</summary>
    public DateTime? ContractEnd { get; set; }

    public int DaysOff { get; set; }
}
