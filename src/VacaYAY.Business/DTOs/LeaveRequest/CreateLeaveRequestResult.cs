namespace VacaYAY.Business.DTOs.LeaveRequest;

public enum CreateLeaveRequestStatus
{
    Created,

    Overlap,

    LeaveTypeNotFound,
}

public record CreateLeaveRequestResult(CreateLeaveRequestStatus Status, LeaveRequestDto? Dto)
{
    public static CreateLeaveRequestResult Created(LeaveRequestDto dto) => new(CreateLeaveRequestStatus.Created, dto);

    public static readonly CreateLeaveRequestResult Overlap = new(CreateLeaveRequestStatus.Overlap, null);

    public static readonly CreateLeaveRequestResult LeaveTypeNotFound = new(CreateLeaveRequestStatus.LeaveTypeNotFound, null);
}
