using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacaYAY.Api.Extensions;
using VacaYAY.Business.DTOs.Common;
using VacaYAY.Business.DTOs.Employee;
using VacaYAY.Business.DTOs.EmployeeImport;
using VacaYAY.Business.Interfaces.Employee;
using VacaYAY.Business.Interfaces.EmployeeImport;

namespace VacaYAY.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IEmployeeImportService _employeeImportService;
    private readonly IValidator<GetEmployeesRequest> _getValidator;
    private readonly IValidator<CreateEmployeeRequest> _createValidator;
    private readonly IValidator<UpdateEmployeeRequest> _updateValidator;
    private readonly IValidator<ImportLegacyEmployeesRequest> _importValidator;

    public EmployeeController(
        IEmployeeService employeeService,
        IEmployeeImportService employeeImportService,
        IValidator<GetEmployeesRequest> getValidator,
        IValidator<CreateEmployeeRequest> createValidator,
        IValidator<UpdateEmployeeRequest> updateValidator,
        IValidator<ImportLegacyEmployeesRequest> importValidator)
    {
        _employeeService = employeeService;
        _employeeImportService = employeeImportService;
        _getValidator = getValidator;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
        _importValidator = importValidator;
    }

    [HttpGet]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<PagedResult<EmployeeDto>>> GetPaged([FromQuery] GetEmployeesRequest request, CancellationToken cancellationToken)
    {
        var validation = await _getValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ToValidationProblem(validation);
        }

        var result = await _employeeService.GetPagedAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("me")]
    public async Task<ActionResult<EmployeeDto>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetMeAsync(User.GetUserId(), cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Employee not found.");
        }

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<EmployeeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Employee not found.");
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<CreateEmployeeResponse>> Create([FromBody] CreateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ToValidationProblem(validation);
        }

        var result = await _employeeService.CreateAsync(request, cancellationToken);
        return result.Status switch
        {
            CreateEmployeeStatus.EmailConflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "An account with this email already exists."),
            CreateEmployeeStatus.ArchivedExists => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "An account with this email is archived.",
                detail: $"Restore employee {result.ArchivedId} to reuse this email."),
            _ => CreatedAtAction(
                nameof(GetById),
                new { id = result.Dto!.Id },
                new CreateEmployeeResponse { Employee = result.Dto!, TempPassword = result.TempPassword! }),
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<EmployeeDto>> Update(int id, [FromBody] UpdateEmployeeRequest request, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ToValidationProblem(validation);
        }

        var result = await _employeeService.UpdateAsync(id, request, cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Employee not found.");
        }

        return Ok(result);
    }

    [HttpPost("{id:int}/reset-password")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<ResetPasswordResult>> ResetPassword(int id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.ResetPasswordAsync(id, cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Employee not found.");
        }

        return Ok(result);
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<EmployeeDto>> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await _employeeService.RestoreAsync(id, cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Archived employee not found.");
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "HrOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _employeeService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Employee not found.");
        }

        return NoContent();
    }

    [HttpGet("legacy")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<IReadOnlyList<LegacyEmployeeRosterItemDto>>> GetLegacyRoster(CancellationToken cancellationToken)
    {
        return Ok(await _employeeImportService.GetRosterAsync(cancellationToken));
    }

    [HttpPost("import-legacy")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<ImportLegacyEmployeesResult>> ImportLegacy([FromBody] ImportLegacyEmployeesRequest request, CancellationToken cancellationToken)
    {
        var validation = await _importValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ToValidationProblem(validation);
        }

        return Ok(await _employeeImportService.ImportAsync(request, cancellationToken));
    }

    private ActionResult ToValidationProblem(FluentValidation.Results.ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return ValidationProblem(ModelState);
    }
}
