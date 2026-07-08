using FluentValidation;
using VacaYAY.Business.DTOs.Auth;

namespace VacaYAY.Business.Validators.Auth;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.CurrentPassword)
            .NotEmpty();

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(128)
            .Matches("[A-Z]").WithMessage("Password must contain an uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain a lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain a digit.")
            .NotEqual(x => x.CurrentPassword)
                .WithMessage("New password must be different from the current password.");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match.");
    }
}
