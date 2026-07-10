using VacaYAY.Business.DTOs.LeaveType;

namespace VacaYAY.Business.Interfaces.LeaveType;


public interface ILeaveTypeService
{
    Task<IReadOnlyList<LeaveTypeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<LeaveTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    Task<CreateLeaveTypeResult> CreateAsync(CreateLeaveTypeRequest request, CancellationToken cancellationToken = default);

    Task<LeaveTypeDto?> UpdateAsync(int id, UpdateLeaveTypeRequest request, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<LeaveTypeDto?> RestoreAsync(int id, CancellationToken cancellationToken = default);
}
