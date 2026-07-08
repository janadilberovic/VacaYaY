namespace VacaYAY.Business.DTOs.Auth;


public class AuthResponse
{
    public string? AccessToken { get; set; }

    public DateTime? ExpiresAtUtc { get; set; }

    //first login with a temporary password
    public bool MustChangePassword { get; set; }

    public AuthUserDto? User { get; set; }
}
