using VacaYAY.Business.DTOs.Employee;

namespace VacaYAY.Business.Interfaces.Employee;

public interface IEmployeeService
{
    Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);

    /// <summary>The caller's own profile (any authenticated user).</summary>
    Task<EmployeeDto?> GetMeAsync(int userId, CancellationToken cancellationToken = default);

    Task<CreateEmployeeResult> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default);

    Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default);

    /// <summary>Regenerates the one-time temp password (show-once) and forces first-login again. Null if not found.</summary>
    Task<ResetPasswordResult?> ResetPasswordAsync(int id, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);

    Task<EmployeeDto?> RestoreAsync(int id, CancellationToken cancellationToken = default);
}
