using Mapster;
using VacaYAY.Business.DTOs.Employee;
using UserEntity = VacaYAY.Domain.Entities.User;

namespace VacaYAY.Business.Mapping.Employee;

public class EmployeeMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserEntity, EmployeeDto>();

        config.NewConfig<CreateEmployeeRequest, UserEntity>();

        // Update only touches the editable fields. Email/Role/HireDate are immutable, and the
        // secrets/identity/soft-delete flags must never be overwritten from a client request.
        config.NewConfig<UpdateEmployeeRequest, UserEntity>()
            .Ignore(u => u.Id)
            .Ignore(u => u.Email)
            .Ignore(u => u.Role)
            .Ignore(u => u.HireDate)
            .Ignore(u => u.PasswordHash!)
            .Ignore(u => u.TempPassword!)
            .Ignore(u => u.IsDeleted)
            .Ignore(u => u.DeletedAt);
    }
}
