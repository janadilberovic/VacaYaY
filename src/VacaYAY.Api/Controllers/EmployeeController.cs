using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacaYAY.Api.Extensions;
using VacaYAY.Business.DTOs.Employee;
using VacaYAY.Business.Interfaces.Employee;

namespace VacaYAY.Api.Controllers;

[ApiController]
[Route("api/employees")]
[Authorize]
public class EmployeeController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IValidator<CreateEmployeeRequest> _createValidator;
    private readonly IValidator<UpdateEmployeeRequest> _updateValidator;

    public EmployeeController(
        IEmployeeService employeeService,
        IValidator<CreateEmployeeRequest> createValidator,
        IValidator<UpdateEmployeeRequest> updateValidator)
    {
        _employeeService = employeeService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<IReadOnlyList<EmployeeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _employeeService.GetAllAsync(cancellationToken);
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

    private ActionResult ToValidationProblem(FluentValidation.Results.ValidationResult validation)
    {
        foreach (var error in validation.Errors)
        {
            ModelState.AddModelError(error.PropertyName, error.ErrorMessage);
        }

        return ValidationProblem(ModelState);
    }
}
