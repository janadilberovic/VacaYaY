namespace VacaYAY.Business.DTOs.LeaveRequest;

public enum CreateLeaveRequestStatus
{
    Created,

    Overlap,

    LeaveTypeNotFound,

    InsufficientBalance,
}

public record CreateLeaveRequestResult(
    CreateLeaveRequestStatus Status,
    LeaveRequestDto? Dto,
    int RequestedDays = 0,
    int RemainingDays = 0)
{
    public static CreateLeaveRequestResult Created(LeaveRequestDto dto) => new(CreateLeaveRequestStatus.Created, dto);

    public static readonly CreateLeaveRequestResult Overlap = new(CreateLeaveRequestStatus.Overlap, null);

    public static readonly CreateLeaveRequestResult LeaveTypeNotFound = new(CreateLeaveRequestStatus.LeaveTypeNotFound, null);

    public static CreateLeaveRequestResult InsufficientBalance(int requestedDays, int remainingDays) =>
        new(CreateLeaveRequestStatus.InsufficientBalance, null, requestedDays, remainingDays);
}
