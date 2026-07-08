using VacaYAY.Business.DTOs.Auth;

namespace VacaYAY.Business.Interfaces.Auth;

/// <summary>Authentication flows: login, first-login / normal password change, and logout.</summary>
public interface IAuthService
{
   
    Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

  
    Task<AuthResponse?> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default);

   
    Task LogoutAsync(string tokenId, DateTime expiresAtUtc, CancellationToken cancellationToken = default);
}
