using VacaYAY.Domain.Entities;

namespace VacaYAY.Business.Interfaces.Auth;


public interface ITokenService
{   
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(User user);
}
