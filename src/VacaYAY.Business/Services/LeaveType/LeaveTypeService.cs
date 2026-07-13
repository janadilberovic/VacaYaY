using Mapster;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.LeaveType;
using VacaYAY.Business.Interfaces.LeaveType;
using VacaYAY.Data;
using LeaveTypeEntity = VacaYAY.Domain.Entities.LeaveType;

namespace VacaYAY.Business.Services.LeaveType;


public class LeaveTypeService : ILeaveTypeService
{
    private readonly VacaYAYDbContext _db;

    public LeaveTypeService(VacaYAYDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<LeaveTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        
        var leaveTypes = await _db.LeaveTypes
        .AsNoTracking()
        .OrderBy(t => t.Name)
        .ToListAsync(cancellationToken);

        var leaveTypeDto= leaveTypes.Adapt<List<LeaveTypeDto>>();

        return leaveTypeDto;
    }

    public async Task<LeaveTypeDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var leaveType = await _db.LeaveTypes.FirstOrDefaultAsync(lt => lt.Id==id,cancellationToken);

        var result=leaveType.Adapt<LeaveTypeDto>();

        return result;
    }

    public async Task<CreateLeaveTypeResult> CreateAsync(CreateLeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        //one active type per name!!
        bool activeMatch = await _db.LeaveTypes.AnyAsync(lt => lt.Name==request.Name, cancellationToken);

        if(activeMatch) { return CreateLeaveTypeResult.Conflict; } //controller maps to 409

        // The unique index on Name spans soft-deleted rows too (MySQL has no filtered indexes),
        // so a name held by an archived type can't just be re-inserted — surface it for restore.
        var archived = await _db.LeaveTypes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(lt => lt.Name == request.Name && lt.IsDeleted, cancellationToken);

        if(archived is not null) { return CreateLeaveTypeResult.Archived(archived.Id); }

        var lt=request.Adapt<LeaveTypeEntity>();

        _db.LeaveTypes.Add(lt);
        await _db.SaveChangesAsync(cancellationToken);

        return CreateLeaveTypeResult.Created(lt.Adapt<LeaveTypeDto>());
    }

    public async Task<LeaveTypeDto?> UpdateAsync(int id, UpdateLeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        var lt= await _db.LeaveTypes.FirstOrDefaultAsync(t=> t.Id==id,cancellationToken);

        if(lt==null) {return null;}

        // Name is immutable, so no uniqueness check is needed here — the request only
        // carries the editable fields (Color/IsPaid/CountsAgainstBalance).
        request.Adapt(lt);

        await _db.SaveChangesAsync(cancellationToken);

        return lt.Adapt<LeaveTypeDto>();
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var lt = await _db.LeaveTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (lt == null) { return false; } 

        //soft delete
        lt.IsDeleted = true;
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<LeaveTypeDto?> RestoreAsync(int id, CancellationToken cancellationToken = default)
    {
        // Bypass the soft-delete filter — we're specifically looking for an archived row.
        var lt = await _db.LeaveTypes
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id && t.IsDeleted, cancellationToken);

        if (lt is null) { return null; }

        lt.IsDeleted = false;
        await _db.SaveChangesAsync(cancellationToken);

        return lt.Adapt<LeaveTypeDto>();
    }
}
