using FluentValidation;
using VacaYAY.Business.DTOs.LeaveType;

namespace VacaYAY.Business.Validators.LeaveType;

public class CreateLeaveTypeRequestValidator : AbstractValidator<CreateLeaveTypeRequest>
{
    public CreateLeaveTypeRequestValidator()
    {
        RuleFor(x => x.Name)
            .IsInEnum().WithMessage("Name must be a valid leave type.");

        // Color is optional (LeaveColor?); only validate the range when a value is supplied.
        RuleFor(x => x.Color!.Value)
            .IsInEnum().WithMessage("Color must be a valid option.")
            .When(x => x.Color is not null);
    }
}
