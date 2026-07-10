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

    public async Task<LeaveTypeDto?> CreateAsync(CreateLeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        //one active type per name!!
        bool isNameTaken = await _db.LeaveTypes.AnyAsync(lt => lt.Name==request.Name, cancellationToken);

        if(isNameTaken) { return null; } //controller maps to 409

        var lt=request.Adapt<LeaveTypeEntity>();

        _db.LeaveTypes.Add(lt);
        await _db.SaveChangesAsync(cancellationToken);

        return lt.Adapt<LeaveTypeDto>();
    }

    public async Task<LeaveTypeDto?> UpdateAsync(int id, UpdateLeaveTypeRequest request, CancellationToken cancellationToken = default)
    {
        var lt= await _db.LeaveTypes.FirstOrDefaultAsync(t=> t.Id==id,cancellationToken);

        if(lt==null) {return null;}

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
}
