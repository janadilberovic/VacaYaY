using FluentValidation;
using VacaYAY.Business.DTOs.EmployeeImport;

namespace VacaYAY.Business.Validators.EmployeeImport;

public class ImportLegacyEmployeesRequestValidator : AbstractValidator<ImportLegacyEmployeesRequest>
{
    public ImportLegacyEmployeesRequestValidator()
    {
        RuleFor(x => x.LegacyIds)
            .NotEmpty().WithMessage("Select at least one employee to import.");

        RuleForEach(x => x.LegacyIds)
            .GreaterThan(0).WithMessage("Legacy employee id must be positive.");
    }
}
