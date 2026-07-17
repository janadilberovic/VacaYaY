namespace VacaYAY.Business.DTOs.Employee;

/// <summary>Outcome of a <see cref="CreateEmployeeRequest"/>, mapped to a status code by the controller.</summary>
public enum CreateEmployeeStatus
{
    Created,

    EmailConflict,

    ArchivedExists,
}


public record CreateEmployeeResult(CreateEmployeeStatus Status, EmployeeDto? Dto, string? TempPassword, int? ArchivedId)
{
    public static CreateEmployeeResult Created(EmployeeDto dto, string tempPassword) =>
        new(CreateEmployeeStatus.Created, dto, tempPassword, null);

    public static readonly CreateEmployeeResult Conflict = new(CreateEmployeeStatus.EmailConflict, null, null, null);

    public static CreateEmployeeResult Archived(int archivedId) =>
        new(CreateEmployeeStatus.ArchivedExists, null, null, archivedId);
}
