using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;

namespace VacaYAY.Api.Extensions;


public static class ClaimsPrincipalExtensions
{
   
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(sub, out var id))
        {
            throw new InvalidOperationException("The principal has no valid 'sub' (user id) claim.");
        }

        return id;
    }

    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(JwtRegisteredClaimNames.Email);
    }

    public static UserRole GetRole(this ClaimsPrincipal principal)
    {
        var role = principal.FindFirstValue(ClaimTypes.Role);
        if (!Enum.TryParse<UserRole>(role, out var parsed))
        {
            throw new InvalidOperationException("The principal has no valid role claim.");
        }

        return parsed;
    }

    public static string? GetTokenId(this ClaimsPrincipal principal)
    {
        return principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
    }

    public static DateTime? GetExpiration(this ClaimsPrincipal principal)
    {
        var exp = principal.FindFirstValue(JwtRegisteredClaimNames.Exp);
        if (long.TryParse(exp, out var seconds))
        {
            return DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime;
        }

        return null;
    }
}
