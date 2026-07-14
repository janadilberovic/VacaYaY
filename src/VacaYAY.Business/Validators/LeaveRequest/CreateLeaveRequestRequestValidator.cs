using FluentValidation;
using VacaYAY.Business.DTOs.LeaveRequest;

namespace VacaYAY.Business.Validators.LeaveRequest;

public class CreateLeaveRequestRequestValidator : AbstractValidator<CreateLeaveRequestRequest>
{
    public CreateLeaveRequestRequestValidator()
    {
        RuleFor(x => x.LeaveTypeId)
            .GreaterThan(0).WithMessage("A leave type must be selected.");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow.Date)
                .WithMessage("Start date cannot be in the past.");

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
                .WithMessage("End date must be on or after the start date.");
    }
}
