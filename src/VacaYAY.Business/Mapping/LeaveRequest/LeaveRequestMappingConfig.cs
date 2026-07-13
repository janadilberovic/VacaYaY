using Mapster;
using VacaYAY.Business.DTOs.LeaveRequest;
using LeaveRequestEntity = VacaYAY.Domain.Entities.LeaveRequest;

namespace VacaYAY.Business.Mapping.LeaveRequest;


public class LeaveRequestMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<LeaveRequestEntity, LeaveRequestDto>()
            .Map(dest => dest.EmployeeName,
                src => src.Employee == null ? string.Empty : src.Employee.FirstName + " " + src.Employee.LastName)
            .Map(dest => dest.LeaveTypeName,
                src => src.LeaveType == null ? default : src.LeaveType.Name)
            .Ignore(dest => dest.WorkingDays);

        config.NewConfig<CreateLeaveRequestRequest, LeaveRequestEntity>();
    }
}
