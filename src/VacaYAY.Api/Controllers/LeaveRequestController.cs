using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VacaYAY.Api.Extensions;
using VacaYAY.Business.DTOs.LeaveRequest;
using VacaYAY.Business.Interfaces.LeaveRequest;

namespace VacaYAY.Api.Controllers;

[ApiController]
[Route("api/leave-requests")]
[Authorize]
public class LeaveRequestController : ControllerBase
{
    private readonly ILeaveRequestService _leaveRequestService;
    private readonly IValidator<CreateLeaveRequestRequest> _createValidator;

    public LeaveRequestController(
        ILeaveRequestService leaveRequestService,
        IValidator<CreateLeaveRequestRequest> createValidator)
    {
        _leaveRequestService = leaveRequestService;
        _createValidator = createValidator;
    }

    [HttpGet]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<IReadOnlyList<LeaveRequestDto>>> GetMine(CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetMineAsync(User.GetUserId(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LeaveRequestDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.GetByIdAsync(id, User.GetUserId(), User.GetRole(), cancellationToken);
        if (result is null)
        {
            return Problem(statusCode: StatusCodes.Status404NotFound, title: "Leave request not found.");
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<LeaveRequestDto>> Create([FromBody] CreateLeaveRequestRequest request, CancellationToken cancellationToken)
    {
        var validation = await _createValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return ToValidationProblem(validation);
        }

        var result = await _leaveRequestService.CreateAsync(User.GetUserId(), request, cancellationToken);
        return result.Status switch
        {
            CreateLeaveRequestStatus.Overlap => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Leave request overlaps an existing request."),
            CreateLeaveRequestStatus.LeaveTypeNotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Leave type not found."),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Dto!.Id }, result.Dto),
        };
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<LeaveRequestDto>> Approve(int id, [FromBody] ReviewLeaveRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.ApproveAsync(id, User.GetUserId(), request, cancellationToken);
        return ToReviewResult(result);
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Policy = "HrOnly")]
    public async Task<ActionResult<LeaveRequestDto>> Reject(int id, [FromBody] ReviewLeaveRequestRequest request, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.RejectAsync(id, User.GetUserId(), request, cancellationToken);
        return ToReviewResult(result);
    }

    [HttpPost("{id:int}/cancel")]
    public async Task<ActionResult<LeaveRequestDto>> Cancel(int id, CancellationToken cancellationToken)
    {
        var result = await _leaveRequestService.CancelAsync(id, User.GetUserId(), cancellationToken);
        return ToReviewResult(result);
    }

    private ActionResult<LeaveRequestDto> ToReviewResult(ReviewLeaveRequestResult result)
    {
        return result.Status switch
        {
            ReviewLeaveRequestStatus.Reviewed => Ok(result.Dto),
            ReviewLeaveRequestStatus.NotFound => Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Leave request not found."),
            ReviewLeaveRequestStatus.InsufficientBalance => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Insufficient leave balance."),
            _ => Problem(
                statusCode: StatusCodes.Status409Conflict,
                title: "Leave request is not pending."),
        };
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
