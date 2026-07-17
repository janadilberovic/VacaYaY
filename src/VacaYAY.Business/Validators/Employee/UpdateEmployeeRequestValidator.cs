using FluentValidation;
using VacaYAY.Business.DTOs.Employee;

namespace VacaYAY.Business.Validators.Employee;

public class UpdateEmployeeRequestValidator : AbstractValidator<UpdateEmployeeRequest>
{
    public UpdateEmployeeRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.DaysOff)
            .GreaterThanOrEqualTo(0).WithMessage("Days off cannot be negative.");
    }
}
