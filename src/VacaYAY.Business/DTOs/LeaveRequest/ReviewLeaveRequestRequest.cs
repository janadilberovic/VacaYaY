namespace VacaYAY.Business.DTOs.LeaveRequest;

/// <summary>
/// HR decision payload for approve/reject. The decision itself is the endpoint;
/// this only carries the optional comment recorded on the request.
/// </summary>
public class ReviewLeaveRequestRequest
{
    public string? HrComment { get; set; }
}
