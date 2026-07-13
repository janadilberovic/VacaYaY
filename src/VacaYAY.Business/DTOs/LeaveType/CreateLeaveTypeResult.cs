namespace VacaYAY.Business.DTOs.LeaveType;

/// <summary>Outcome of a <see cref="CreateLeaveTypeRequest"/>, mapped to a status code by the controller.</summary>
public enum CreateLeaveTypeStatus
{
    /// <summary>Type was created.</summary>
    Created,

    /// <summary>An active type already uses this name → 409.</summary>
    NameConflict,

    /// <summary>A soft-deleted type uses this name; it can be restored instead → 409 with the archived id.</summary>
    ArchivedExists,
}

/// <summary>
/// Result of <c>CreateAsync</c>. Carries the created DTO on success, or the archived type's id
/// when the name belongs to a soft-deleted type that HR can restore.
/// </summary>
public record CreateLeaveTypeResult(CreateLeaveTypeStatus Status, LeaveTypeDto? Dto, int? ArchivedId)
{
    public static CreateLeaveTypeResult Created(LeaveTypeDto dto) => new(CreateLeaveTypeStatus.Created, dto, null);

    public static readonly CreateLeaveTypeResult Conflict = new(CreateLeaveTypeStatus.NameConflict, null, null);

    public static CreateLeaveTypeResult Archived(int archivedId) => new(CreateLeaveTypeStatus.ArchivedExists, null, archivedId);
}
