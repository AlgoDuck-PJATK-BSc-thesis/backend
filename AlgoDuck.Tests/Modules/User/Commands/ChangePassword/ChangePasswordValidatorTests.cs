using AlgoDuck.Modules.User.Commands.ChangePassword;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Commands.ChangePassword;

public sealed class ChangePasswordValidatorTests
{
    [Fact]
    public void Validate_WhenCurrentPasswordEmpty_ThenHasValidationError()
    {
        var validator = new ChangePasswordValidator();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "",
            NewPassword = "newpassword"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Validate_WhenCurrentPasswordTooShort_ThenHasValidationError()
    {
        var validator = new ChangePasswordValidator();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "12345",
            NewPassword = "newpassword"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Validate_WhenNewPasswordEmpty_ThenHasValidationError()
    {
        var validator = new ChangePasswordValidator();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "123456",
            NewPassword = ""
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_WhenNewPasswordTooShort_ThenHasValidationError()
    {
        var validator = new ChangePasswordValidator();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "123456",
            NewPassword = "1234567"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NewPassword);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new ChangePasswordValidator();
        var dto = new ChangePasswordDto
        {
            CurrentPassword = "123456",
            NewPassword = "12345678"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}