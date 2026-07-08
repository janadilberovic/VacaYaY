using FluentValidation;
using VacaYAY.Business.DTOs.Auth;

namespace VacaYAY.Business.Validators.Auth;


public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256); // matches User.Email column length

        RuleFor(x => x.Password)
            .NotEmpty()
            .MaximumLength(128); // guard against oversized inputs hitting the hasher
    }
}
