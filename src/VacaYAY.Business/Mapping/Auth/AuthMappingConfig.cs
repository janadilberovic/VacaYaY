using Mapster;
using VacaYAY.Business.DTOs.Auth;
using VacaYAY.Domain.Entities;

namespace VacaYAY.Business.Mapping.Auth;


public class AuthMappingConfig : IRegister
{
    //later in code i will use method adapt 
    //eg  AuthUserDto userDto=user.Adapt<AuthUserDto>();
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, AuthUserDto>();
    }
}
