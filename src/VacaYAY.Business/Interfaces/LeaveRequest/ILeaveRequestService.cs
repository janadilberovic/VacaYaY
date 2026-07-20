using VacaYAY.Business.DTOs.LeaveRequest;

namespace VacaYAY.Business.Interfaces.LeaveRequest;

public interface ILeaveRequestService
{
    Task<IReadOnlyList<LeaveRequestDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<LeaveRequestDto>> GetMineAsync(int employeeId, CancellationToken cancellationToken = default);

    Task<LeaveRequestDto?> GetByIdAsync(int id, int requestingUserId, UserRole role, CancellationToken cancellationToken = default);

    Task<CreateLeaveRequestResult> CreateAsync(int employeeId, CreateLeaveRequestRequest request, CancellationToken cancellationToken = default);

    Task<ReviewLeaveRequestResult> ApproveAsync(int id, int hrUserId, ReviewLeaveRequestRequest request, CancellationToken cancellationToken = default);

    Task<ReviewLeaveRequestResult> RejectAsync(int id, int hrUserId, ReviewLeaveRequestRequest request, CancellationToken cancellationToken = default);

    Task<ReviewLeaveRequestResult> CancelAsync(int id, int employeeId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DateOnly>> GetHolidaysAsync(int year, CancellationToken cancellationToken = default);
}
