using System.Security.Cryptography;
using System.Text;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VacaYAY.Business.DTOs.Auth;
using VacaYAY.Business.Interfaces.Auth;
using VacaYAY.Data;
using VacaYAY.Domain.Entities;

namespace VacaYAY.Business.Services.Auth;

/// <summary>
/// Application-layer authentication logic: login, first-login / normal password change,
/// and logout. Reuses <see cref="IPasswordHasher{TUser}"/> for hashing and
/// <see cref="ITokenService"/> for issuing JWTs. Returns DTOs only — the <see cref="User"/>
/// entity (with its secrets) never leaves this layer.
/// </summary>
public class AuthService : IAuthService
{
    private readonly VacaYAYDbContext _db;
    private readonly IPasswordHasher<User> _hasher;
    private readonly ITokenService _tokenService;
    private readonly ITokenDenylist _denylist;

    public AuthService(
        VacaYAYDbContext db,
        IPasswordHasher<User> hasher,
        ITokenService tokenService,
        ITokenDenylist denylist)
    {
        _db = db;
        _hasher = hasher;
        _tokenService = tokenService;
        _denylist = denylist;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        // The global query filter already excludes soft-deleted users.
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        // Unknown or deactivated account — stay vague so we don't reveal which.
        if (user is null || !user.IsActive)
        {
            return null;
        }

        // First login: HR issued a temp password and the user hasn't set their own yet.
        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            if (VerifyTempPassword(user, request.Password))
            {
                // Correct temp password, but force a real password before issuing a token.
                return new AuthResponse
                {
                    MustChangePassword = true,
                    //mapping service
                    User = user.Adapt<AuthUserDto>(),
                };
            }

            return null;
        }

        // Normal login: verify against the stored hash.
        var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
        {
            return null;
        }

        // The hasher signals when its parameters are outdated — transparently upgrade.
        if (result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _hasher.HashPassword(user, request.Password);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return BuildTokenResponse(user);
    }

    public async Task<AuthResponse?> ChangePasswordAsync(ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null || !user.IsActive)
        {
            return null;
        }

        // Verify the password being replaced: temp password on first login, else the current hash.
        bool verified = string.IsNullOrEmpty(user.PasswordHash)
            ? VerifyTempPassword(user, request.CurrentPassword)
            : _hasher.VerifyHashedPassword(user, user.PasswordHash, request.CurrentPassword) != PasswordVerificationResult.Failed;

        if (!verified)
        {
            return null;
        }

        // Set the new password and burn the one-time temp password.
        user.PasswordHash = _hasher.HashPassword(user, request.NewPassword);
        user.TempPassword = null;
        await _db.SaveChangesAsync(cancellationToken);

        return BuildTokenResponse(user);
    }

    public Task LogoutAsync(string tokenId, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        _denylist.Revoke(tokenId, expiresAtUtc);
        return Task.CompletedTask;
    }

    private AuthResponse BuildTokenResponse(User user)
    {
        var (token, expiresAtUtc) = _tokenService.CreateAccessToken(user);
        return new AuthResponse
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            MustChangePassword = false,
            User = user.Adapt<AuthUserDto>(),
        };
    }

    // Temp passwords are stored as plaintext by design; compare in constant time so a
    // correct prefix can't be inferred from response timing.
    private static bool VerifyTempPassword(User user, string candidate)
    {
        if (string.IsNullOrEmpty(user.TempPassword))
        {
            return false;
        }

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(user.TempPassword),
            Encoding.UTF8.GetBytes(candidate));
    }
}
