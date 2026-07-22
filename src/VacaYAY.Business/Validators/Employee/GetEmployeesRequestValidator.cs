using FluentValidation;
using VacaYAY.Business.DTOs.Employee;

namespace VacaYAY.Business.Validators.Employee;

public class GetEmployeesRequestValidator : AbstractValidator<GetEmployeesRequest>
{
    public GetEmployeesRequestValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1).WithMessage("Page must be 1 or greater.");

        // Upper bound is the point of paging — without it ?pageSize=100000 fetches the whole table.
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100).WithMessage("Page size must be between 1 and 100.");
    }
}
