using Mapster;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.Employee;
using VacaYAY.Business.Interfaces.Employee;
using VacaYAY.Data;
using UserEntity = VacaYAY.Domain.Entities.User;

namespace VacaYAY.Business.Services.Employee;

/// <summary>
/// HR-facing employee management over the <c>User</c> entity: list, read, provision, edit,
/// soft-delete and restore. Returns DTOs only — <c>PasswordHash</c>/<c>TempPassword</c> never leave
/// this layer.
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly VacaYAYDbContext _db;

    public EmployeeService(VacaYAYDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<EmployeeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var employees = await _db.Users
            .AsNoTracking()
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .ToListAsync(cancellationToken);

        return employees.Adapt<List<EmployeeDto>>();
    }

    public async Task<EmployeeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return user.Adapt<EmployeeDto>();
    }

    public Task<EmployeeDto?> GetMeAsync(int userId, CancellationToken cancellationToken = default)
        => GetByIdAsync(userId, cancellationToken);

    public async Task<CreateEmployeeResult> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        // One active account per email.
        bool activeMatch = await _db.Users.AnyAsync(u => u.Email == request.Email, cancellationToken);
        if (activeMatch) { return CreateEmployeeResult.Conflict; } // controller maps to 409

        // The unique index on Email spans soft-deleted rows too (MySQL has no filtered indexes),
        // so an email held by an archived account can't just be re-inserted — surface it for restore.
        var archived = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsDeleted, cancellationToken);

        if (archived is not null) { return CreateEmployeeResult.Archived(archived.Id); }

        var user = request.Adapt<UserEntity>();
        var tempPassword = TempPasswordGenerator.Generate();
        user.TempPassword = tempPassword; // plaintext by design (existing Auth model); burned on first change
        user.IsActive = true;
        user.PasswordHash = null; // no real password yet — first login uses TempPassword, then it's replaced

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        // Surface the temp password once, here only — EmployeeDto and every read endpoint stay clean.
        return CreateEmployeeResult.Created(user.Adapt<EmployeeDto>(), tempPassword);
    }

    public async Task<EmployeeDto?> UpdateAsync(int id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null) { return null; }

        // Email/Role/HireDate are immutable — the mapping config ignores them (and the secrets).
        request.Adapt(user);

        await _db.SaveChangesAsync(cancellationToken);

        return user.Adapt<EmployeeDto>();
    }

    public async Task<ResetPasswordResult?> ResetPasswordAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null) { return null; }

        // Mint a new one-time temp password and drop the account back into the first-login flow.
        var tempPassword = TempPasswordGenerator.Generate();
        user.TempPassword = tempPassword;
        user.PasswordHash = null;
        await _db.SaveChangesAsync(cancellationToken);

        return new ResetPasswordResult(user.Id, tempPassword);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        if (user is null) { return false; }

        // Soft delete: keep the row for history, drop it from lists and block login.
        user.IsDeleted = true;
        user.DeletedAt = DateTime.UtcNow;
        user.IsActive = false;
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<EmployeeDto?> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        // Bypass the soft-delete filter — we're specifically looking for an archived row.
        var user = await _db.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == id && u.IsDeleted, cancellationToken);

        if (user is null) { return null; }

        user.IsDeleted = false;
        user.DeletedAt = null;
        user.IsActive = true;
        await _db.SaveChangesAsync(cancellationToken);

        return user.Adapt<EmployeeDto>();
    }
}
