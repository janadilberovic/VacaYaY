using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacaYAY.Business.DTOs.LeaveType;
using VacaYAY.Business.Interfaces.LeaveType;

namespace VacaYAY.Api.Controllers;

[ApiController]
[Route("api/leave-types")]
[Authorize]
public class LeaveTypeController : ControllerBase
{
    private readonly ILeaveTypeService _leaveTypeService;
    private readonly IValidator<CreateLeaveTypeRequest> _createValidator;
    private readonly IValidator<UpdateLeaveTypeRequest> _updateValidator;

    public LeaveTypeController(
        ILeaveTypeService leaveTypeService,
        IValidator<CreateLeaveTypeRequest> createValidator,
        IValidator<UpdateLeaveTypeRequest> updateValidator)
    {
        _leaveTypeService = leaveTypeService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LeaveTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _leaveTypeService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeaveTypeDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _leaveTypeService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Leave type not found.");
        }

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Policy = "HrOnly")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ActionResult<LeaveTypeDto>> Create([FromForm] CreateLeaveTypeRequest request, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ToValidationProblem(validation);
        }

        var result = await _leaveTypeService.CreateAsync(request, cancellationToken);
        return result.Status switch
        {
            CreateLeaveTypeStatus.NameConflict => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "A leave type with this name already exists."),
            CreateLeaveTypeStatus.ArchivedExists => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "A leave type with this name is archived.",
                detail: $"Restore leave type {result.ArchivedId} to reuse this name."),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Dto!.Id }, result.Dto),
        };
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "HrOnly")]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<ActionResult<LeaveTypeDto>> Update(int id, [FromForm] UpdateLeaveTypeRequest request, CancellationToken cancellationToken)
    {
        var validation = await _updateValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ToValidationProblem(validation);
        }

        var result = await _leaveTypeService.UpdateAsync(id, request, cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Leave type not found.");
        }

        return Ok(result);
    }

    [HttpPost("{id:int}/restore")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<LeaveTypeDto>> Restore(int id, CancellationToken cancellationToken)
    {
        var result = await _leaveTypeService.RestoreAsync(id, cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Archived leave type not found.");
        }

        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "HrOnly")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _leaveTypeService.DeleteAsync(id, cancellationToken);
        if (!deleted)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Leave type not found.");
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
