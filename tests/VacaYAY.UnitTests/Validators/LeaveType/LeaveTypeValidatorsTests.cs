using VacaYAY.Business.DTOs.LeaveType;
using VacaYAY.Business.Validators.LeaveType;

namespace VacaYAY.UnitTests;

public class CreateLeaveTypeRequestValidatorTests
{
    private readonly CreateLeaveTypeRequestValidator _validator = new();

    [Fact]
    public void Passes_WhenRequestIsValid()
    {
        // Given
        var request = new CreateLeaveTypeRequest { Name = LeaveTypeName.Annual, Color = LeaveColor.Green };

        // When / Then
        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Passes_WhenColorIsNull()
    {
        // Given — Color is optional; the range rule only fires when a value is present
        var request = new CreateLeaveTypeRequest { Name = LeaveTypeName.Annual, Color = null };

        // When / Then
        Assert.True(_validator.Validate(request).IsValid);
    }

    [Fact]
    public void Fails_WhenNameIsNotAValidEnumValue()
    {
        // Given
        var request = new CreateLeaveTypeRequest { Name = (LeaveTypeName)999 };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLeaveTypeRequest.Name));
    }

    [Fact]
    public void Fails_WhenColorIsNotAValidEnumValue()
    {
        // Given
        var request = new CreateLeaveTypeRequest { Name = LeaveTypeName.Annual, Color = (LeaveColor)999 };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.False(result.IsValid);
    }
}

public class UpdateLeaveTypeRequestValidatorTests
{
    private readonly UpdateLeaveTypeRequestValidator _validator = new();

    [Fact]
    public void Passes_WhenColorIsValid()
    {
        Assert.True(_validator.Validate(new UpdateLeaveTypeRequest { Color = LeaveColor.Blue }).IsValid);
    }

    [Fact]
    public void Passes_WhenColorIsNull()
    {
        Assert.True(_validator.Validate(new UpdateLeaveTypeRequest { Color = null }).IsValid);
    }

    [Fact]
    public void Fails_WhenColorIsNotAValidEnumValue()
    {
        // Given
        var request = new UpdateLeaveTypeRequest { Color = (LeaveColor)999 };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.False(result.IsValid);
    }
}
