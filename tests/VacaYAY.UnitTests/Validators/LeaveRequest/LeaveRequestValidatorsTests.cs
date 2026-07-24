using VacaYAY.Business.DTOs.LeaveRequest;
using VacaYAY.Business.Validators.LeaveRequest;

namespace VacaYAY.UnitTests;

public class CreateLeaveRequestRequestValidatorTests
{
    private readonly CreateLeaveRequestRequestValidator _validator = new();

    private static CreateLeaveRequestRequest Valid() => new()
    {
        LeaveTypeId = 1,
        StartDate = DateTime.UtcNow.Date.AddDays(1),
        EndDate = DateTime.UtcNow.Date.AddDays(5),
    };

    [Fact]
    public void Passes_WhenRequestIsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Fails_WhenLeaveTypeIsNotSelected(int leaveTypeId)
    {
        // Given
        var request = Valid();
        request.LeaveTypeId = leaveTypeId;

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLeaveRequestRequest.LeaveTypeId));
    }

    [Fact]
    public void Fails_WhenStartDateIsInThePast()
    {
        // Given
        var request = Valid();
        request.StartDate = DateTime.UtcNow.Date.AddDays(-1);

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLeaveRequestRequest.StartDate));
    }

    [Fact]
    public void Fails_WhenEndDateIsBeforeStartDate()
    {
        // Given
        var request = Valid();
        request.StartDate = DateTime.UtcNow.Date.AddDays(5);
        request.EndDate = DateTime.UtcNow.Date.AddDays(3);

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLeaveRequestRequest.EndDate));
    }
}

public class GetLeaveRequestsRequestValidatorTests
{
    private readonly GetLeaveRequestsRequestValidator _validator = new();

    [Fact]
    public void Passes_WithDefaultRequest()
    {
        Assert.True(_validator.Validate(new GetLeaveRequestsRequest()).IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Fails_WhenPageIsBelowOne(int page)
    {
        // Given
        var request = new GetLeaveRequestsRequest { Page = page };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetLeaveRequestsRequest.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Fails_WhenPageSizeIsOutOfRange(int pageSize)
    {
        // Given
        var request = new GetLeaveRequestsRequest { PageSize = pageSize };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetLeaveRequestsRequest.PageSize));
    }

    [Fact]
    public void Fails_WhenStatusIsNotAValidEnumValue()
    {
        // Given
        var request = new GetLeaveRequestsRequest { Status = (LeaveRequestStatus)999 };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetLeaveRequestsRequest.Status));
    }

    [Fact]
    public void Fails_WhenLeaveTypeNameIsNotAValidEnumValue()
    {
        // Given
        var request = new GetLeaveRequestsRequest { LeaveTypeName = (LeaveTypeName)999 };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetLeaveRequestsRequest.LeaveTypeName));
    }

    [Fact]
    public void Fails_WhenSortByIsNotAValidEnumValue()
    {
        // Given
        var request = new GetLeaveRequestsRequest { SortBy = (LeaveRequestSortField)999 };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetLeaveRequestsRequest.SortBy));
    }
}
