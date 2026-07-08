namespace VacaYAY.Business.DTOs.Auth;


public class AuthUserDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; }

    public string? Department { get; set; }

    public string? JobTitle { get; set; }

    public int DaysOff { get; set; }

    public string? ProfileImageUrl { get; set; }
}
