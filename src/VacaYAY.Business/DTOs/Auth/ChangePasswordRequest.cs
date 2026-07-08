namespace VacaYAY.Business.DTOs.Auth;


public class ChangePasswordRequest
{
    public string Email { get; set; } = string.Empty;

    // tmp password issued by hr for the first login
    public string CurrentPassword { get; set; } = string.Empty;

    public string NewPassword { get; set; } = string.Empty;

    public string ConfirmNewPassword { get; set; } = string.Empty;
}
