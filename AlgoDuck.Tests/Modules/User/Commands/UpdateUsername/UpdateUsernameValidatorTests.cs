using AlgoDuck.Modules.User.Commands.UpdateUsername;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Commands.UpdateUsername;

public sealed class UpdateUsernameValidatorTests
{
    [Fact]
    public void Validate_WhenNewUserNameEmpty_ThenHasValidationError()
    {
        var validator = new UpdateUsernameValidator();
        var dto = new UpdateUsernameDto
        {
            NewUserName = ""
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NewUserName);
    }

    [Fact]
    public void Validate_WhenNewUserNameTooShort_ThenHasValidationError()
    {
        var validator = new UpdateUsernameValidator();
        var dto = new UpdateUsernameDto
        {
            NewUserName = "ab"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NewUserName);
    }

    [Fact]
    public void Validate_WhenNewUserNameTooLong_ThenHasValidationError()
    {
        var validator = new UpdateUsernameValidator();
        var dto = new UpdateUsernameDto
        {
            NewUserName = new string('a', 33)
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NewUserName);
    }

    [Fact]
    public void Validate_WhenNewUserNameHasInvalidCharacters_ThenHasValidationError()
    {
        var validator = new UpdateUsernameValidator();
        var dto = new UpdateUsernameDto
        {
            NewUserName = "bad-name"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.NewUserName);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new UpdateUsernameValidator();
        var dto = new UpdateUsernameDto
        {
            NewUserName = "good_name_123"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}