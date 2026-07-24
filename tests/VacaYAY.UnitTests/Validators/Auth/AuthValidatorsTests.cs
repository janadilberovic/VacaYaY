using VacaYAY.Business.DTOs.Auth;
using VacaYAY.Business.Validators.Auth;

namespace VacaYAY.UnitTests;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    private static LoginRequest Valid() => new()
    {
        Email = "ana@vacayay.test",
        Password = "secret",
    };

    [Fact]
    public void Passes_WhenRequestIsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
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
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public void Fails_WhenEmailExceeds256Characters()
    {
        // Given — valid shape, but longer than the User.Email column
        var request = Valid();
        request.Email = new string('a', 250) + "@test.com";

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Email));
    }

    [Fact]
    public void Fails_WhenPasswordIsEmpty()
    {
        // Given
        var request = Valid();
        request.Password = "";

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Password));
    }

    [Fact]
    public void Fails_WhenPasswordExceeds128Characters()
    {
        // Given
        var request = Valid();
        request.Password = new string('a', 129);

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(LoginRequest.Password));
    }
}

public class ChangePasswordRequestValidatorTests
{
    private readonly ChangePasswordRequestValidator _validator = new();

    private static ChangePasswordRequest Valid() => new()
    {
        Email = "ana@vacayay.test",
        CurrentPassword = "OldPass1",
        NewPassword = "NewPass1",
        ConfirmNewPassword = "NewPass1",
    };

    [Fact]
    public void Passes_WhenRequestIsValid()
    {
        Assert.True(_validator.Validate(Valid()).IsValid);
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
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.Email));
    }

    [Fact]
    public void Fails_WhenCurrentPasswordIsEmpty()
    {
        // Given
        var request = Valid();
        request.CurrentPassword = "";

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.CurrentPassword));
    }

    [Fact]
    public void Fails_WhenNewPasswordIsTooShort()
    {
        // Given
        var request = Valid();
        request.NewPassword = "Ab1";
        request.ConfirmNewPassword = "Ab1";

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }

    [Theory]
    [InlineData("newpass1")]  // no uppercase
    [InlineData("NEWPASS1")]  // no lowercase
    [InlineData("NewPassword")]  // no digit
    public void Fails_WhenNewPasswordMissingCharacterClass(string newPassword)
    {
        // Given
        var request = Valid();
        request.NewPassword = newPassword;
        request.ConfirmNewPassword = newPassword;

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }

    [Fact]
    public void Fails_WhenNewPasswordEqualsCurrentPassword()
    {
        // Given
        var request = Valid();
        request.NewPassword = request.CurrentPassword;
        request.ConfirmNewPassword = request.CurrentPassword;

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.NewPassword));
    }

    [Fact]
    public void Fails_WhenConfirmationDoesNotMatch()
    {
        // Given
        var request = Valid();
        request.ConfirmNewPassword = "Different1";

        // When
        var result = _validator.Validate(request);

        // Then
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(ChangePasswordRequest.ConfirmNewPassword));
    }
}
