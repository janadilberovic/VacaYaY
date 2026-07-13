namespace VacaYAY.Business.DTOs.LeaveRequest;


public enum ReviewLeaveRequestStatus
{

    Reviewed,


    NotFound,


    NotPending,


    InsufficientBalance,
}


public record ReviewLeaveRequestResult(ReviewLeaveRequestStatus Status, LeaveRequestDto? Dto)
{
    public static ReviewLeaveRequestResult Reviewed(LeaveRequestDto dto) => new(ReviewLeaveRequestStatus.Reviewed, dto);

    public static readonly ReviewLeaveRequestResult NotFound = new(ReviewLeaveRequestStatus.NotFound, null);

    public static readonly ReviewLeaveRequestResult NotPending = new(ReviewLeaveRequestStatus.NotPending, null);

    public static readonly ReviewLeaveRequestResult InsufficientBalance = new(ReviewLeaveRequestStatus.InsufficientBalance, null);
}
