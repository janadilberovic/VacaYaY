using Mapster;
using VacaYAY.Business.DTOs.LeaveType;
using LeaveTypeEntity = VacaYAY.Domain.Entities.LeaveType;

namespace VacaYAY.Business.Mapping.LeaveType;


public class LeaveTypeMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LeaveTypeEntity, LeaveTypeDto>();
        config.NewConfig<CreateLeaveTypeRequest, LeaveTypeEntity>();
        config.NewConfig<UpdateLeaveTypeRequest, LeaveTypeEntity>();
    }
}
