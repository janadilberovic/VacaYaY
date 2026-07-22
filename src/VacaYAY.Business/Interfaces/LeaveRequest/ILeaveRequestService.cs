using VacaYAY.Business.DTOs.Common;
using VacaYAY.Business.DTOs.LeaveRequest;

namespace VacaYAY.Business.Interfaces.LeaveRequest;

public interface ILeaveRequestService
{
    Task<PagedResult<LeaveRequestDto>> GetPagedAsync(GetLeaveRequestsRequest request, CancellationToken cancellationToken = default);

    Task<PagedResult<LeaveRequestDto>> GetMinePagedAsync(int employeeId, GetLeaveRequestsRequest request, CancellationToken cancellationToken = default);

    Task<LeaveRequestSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);

    Task<LeaveRequestDto?> GetByIdAsync(int id, int requestingUserId, UserRole role, CancellationToken cancellationToken = default);

    Task<LeaveBalanceDto> GetBalanceAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<CreateLeaveRequestResult> CreateAsync(int employeeId, CreateLeaveRequestRequest request, CancellationToken cancellationToken = default);

    Task<ReviewLeaveRequestResult> ApproveAsync(int id, int hrUserId, ReviewLeaveRequestRequest request, CancellationToken cancellationToken = default);

    Task<ReviewLeaveRequestResult> RejectAsync(int id, int hrUserId, ReviewLeaveRequestRequest request, CancellationToken cancellationToken = default);

    Task<ReviewLeaveRequestResult> CancelAsync(int id, int employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateOnly>> GetHolidaysAsync(int year, CancellationToken cancellationToken = default);
}
