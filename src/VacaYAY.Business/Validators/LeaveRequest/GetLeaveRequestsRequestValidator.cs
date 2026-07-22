using FluentValidation;
using VacaYAY.Business.DTOs.LeaveRequest;

namespace VacaYAY.Business.Validators.LeaveRequest;

public class GetLeaveRequestsRequestValidator : AbstractValidator<GetLeaveRequestsRequest>
{
    public GetLeaveRequestsRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

        // Upper bound is the point of paging — without it ?pageSize=100000 fetches the whole table.
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");

        RuleFor(x => x.LeaveTypeName)
            .IsInEnum().WithMessage("Unknown leave type.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("Unknown status.");

        RuleFor(x => x.SortBy)
            .IsInEnum().WithMessage("Unknown sort field.");
    }
}
