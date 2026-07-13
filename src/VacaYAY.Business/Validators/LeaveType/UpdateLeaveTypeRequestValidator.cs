using FluentValidation;
using VacaYAY.Business.DTOs.LeaveType;

namespace VacaYAY.Business.Validators.LeaveType;

public class UpdateLeaveTypeRequestValidator : AbstractValidator<UpdateLeaveTypeRequest>
{
    public UpdateLeaveTypeRequestValidator()
    {
        // Name is immutable on update, so it is not part of the update request.

        // Color is optional (LeaveColor?); only validate the range when a value is supplied.
        RuleFor(x => x.Color!.Value)
            .IsInEnum().WithMessage("Color must be a valid option.")
            .When(x => x.Color is not null);
    }
}
