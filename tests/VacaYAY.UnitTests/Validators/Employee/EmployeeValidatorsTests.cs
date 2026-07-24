using VacaYAY.Business.DTOs.Employee;
using VacaYAY.Business.DTOs.EmployeeImport;
using VacaYAY.Business.Validators.Employee;
using VacaYAY.Business.Validators.EmployeeImport;

namespace VacaYAY.UnitTests;

public class CreateEmployeeRequestValidatorTests
{
    private readonly CreateEmployeeRequestValidator _validator = new();

    private static CreateEmployeeRequest Valid() => new()
    {
        FirstName = "Ana",
        LastName = "Zoric",
        Email = "ana@vacayay.test",
        Role = UserRole.Employee,
        DaysOff = 20,
    };

    [Fact]
    public void Passes_WhenRequestIsValid()
    {
        // Given
        var request = Valid();

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fails_WhenFirstNameIsEmpty(string firstName)
    {
        // Given
        var request = Valid();
        request.FirstName = firstName;

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateEmployeeRequest.FirstName));
    }

    [Fact]
    public void Fails_WhenFirstNameExceeds100Characters()
    {
        // Given
        var request = Valid();
        request.FirstName = new string('a', 101);

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateEmployeeRequest.FirstName));
    }

    [Fact]
    public void Fails_WhenLastNameIsEmpty()
    {
        // Given
        var request = Valid();
        request.LastName = "";

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateEmployeeRequest.LastName));
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Fails_WhenEmailIsMissingOrMalformed(string email)
    {
        // Given
        var request = Valid();
        request.Email = email;

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateEmployeeRequest.Email));
    }

    [Fact]
    public void Fails_WhenRoleIsNotAValidEnumValue()
    {
        // Given
        var request = Valid();
        request.Role = (UserRole)999;

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateEmployeeRequest.Role));
    }

    [Fact]
    public void Fails_WhenDaysOffIsNegative()
    {
        // Given
        var request = Valid();
        request.DaysOff = -1;

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateEmployeeRequest.DaysOff));
    }
}

public class UpdateEmployeeRequestValidatorTests
{
    private readonly UpdateEmployeeRequestValidator _validator = new();

    private static UpdateEmployeeRequest Valid() => new()
    {
        FirstName = "Ana",
        LastName = "Zoric",
        DaysOff = 20,
    };

    [Fact]
    public void Passes_WhenRequestIsValid()
    {
        // Given
        var request = Valid();

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Fails_WhenFirstNameIsEmpty()
    {
        // Given
        var request = Valid();
        request.FirstName = "";

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateEmployeeRequest.FirstName));
    }

    [Fact]
    public void Fails_WhenLastNameExceeds100Characters()
    {
        // Given
        var request = Valid();
        request.LastName = new string('a', 101);

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateEmployeeRequest.LastName));
    }

    [Fact]
    public void Fails_WhenDaysOffIsNegative()
    {
        // Given
        var request = Valid();
        request.DaysOff = -5;

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateEmployeeRequest.DaysOff));
    }
}

public class GetEmployeesRequestValidatorTests
{
    private readonly GetEmployeesRequestValidator _validator = new();

    [Fact]
    public void Passes_WhenPagingIsWithinBounds()
    {
        // Given
        var request = new GetEmployeesRequest { Page = 1, PageSize = 20 };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Fails_WhenPageIsBelowOne(int page)
    {
        // Given
        var request = new GetEmployeesRequest { Page = page, PageSize = 20 };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetEmployeesRequest.Page));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void Fails_WhenPageSizeIsOutOfRange(int pageSize)
    {
        // Given
        var request = new GetEmployeesRequest { Page = 1, PageSize = pageSize };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetEmployeesRequest.PageSize));
    }
}

public class ImportLegacyEmployeesRequestValidatorTests
{
    private readonly ImportLegacyEmployeesRequestValidator _validator = new();

    [Fact]
    public void Passes_WhenIdsArePositive()
    {
        // Given
        var request = new ImportLegacyEmployeesRequest { LegacyIds = new() { 1, 2, 3 } };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Fails_WhenNoIdsSelected()
    {
        // Given
        var request = new ImportLegacyEmployeesRequest { LegacyIds = new() };

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ImportLegacyEmployeesRequest.LegacyIds));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public void Fails_WhenAnyIdIsNotPositive(int badId)
    {
        // Given
        var request = new ImportLegacyEmployeesRequest { LegacyIds = new() { 1, badId } };

        // When
        var result = _validator.Validate(request);

        // Then — RuleForEach reports the offending element
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName.StartsWith(nameof(ImportLegacyEmployeesRequest.LegacyIds)));
    }
}
