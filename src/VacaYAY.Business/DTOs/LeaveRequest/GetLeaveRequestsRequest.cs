namespace VacaYAY.Business.DTOs.LeaveRequest;

public class GetLeaveRequestsRequest
{
    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public LeaveRequestStatus? Status { get; set; }

    public int? EmployeeId { get; set; }

    public LeaveTypeName? LeaveTypeName { get; set; }

    public LeaveRequestSortField SortBy { get; set; } = LeaveRequestSortField.Default;

    public bool SortDescending { get; set; } = true;
}
