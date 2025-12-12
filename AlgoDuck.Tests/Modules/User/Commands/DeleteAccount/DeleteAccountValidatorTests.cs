using AlgoDuck.Modules.User.Commands.DeleteAccount;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Modules.User.Commands.DeleteAccount;

public sealed class DeleteAccountValidatorTests
{
    [Fact]
    public void Validate_WhenCurrentPasswordEmpty_ThenHasValidationError()
    {
        var validator = new DeleteAccountValidator();
        var dto = new DeleteAccountDto
        {
            CurrentPassword = ""
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Validate_WhenCurrentPasswordTooShort_ThenHasValidationError()
    {
        var validator = new DeleteAccountValidator();
        var dto = new DeleteAccountDto
        {
            CurrentPassword = "12345"
        };

        var result = validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.CurrentPassword);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var validator = new DeleteAccountValidator();
        var dto = new DeleteAccountDto
        {
            CurrentPassword = "123456"
        };

        var result = validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}