using FluentValidation;
using VacaYAY.Business.DTOs.Employee;

namespace VacaYAY.Business.Validators.Employee;

public class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid address.")
            .MaximumLength(256);

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role must be a valid option.");

        RuleFor(x => x.DaysOff)
            .GreaterThanOrEqualTo(0).WithMessage("Days off cannot be negative.");

    }
}
