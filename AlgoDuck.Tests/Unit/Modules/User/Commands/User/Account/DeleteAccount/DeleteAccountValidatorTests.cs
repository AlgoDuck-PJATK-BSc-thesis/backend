using AlgoDuck.Modules.User.Commands.User.Account.DeleteAccount;
using FluentValidation.TestHelper;

namespace AlgoDuck.Tests.Unit.Modules.User.Commands.User.Account.DeleteAccount;

public sealed class DeleteAccountValidatorTests
{
    private const string ConfirmationPhrase = "I am sure I want to delete my account";

    readonly DeleteAccountValidator _validator = new();

    [Fact]
    public void Validate_WhenConfirmationTextEmpty_ThenHasValidationError()
    {
        var dto = new DeleteAccountDto
        {
            ConfirmationText = ""
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmationText);
    }

    [Fact]
    public void Validate_WhenConfirmationTextWrong_ThenHasValidationError()
    {
        var dto = new DeleteAccountDto
        {
            ConfirmationText = "I am sure"
        };

        var result = _validator.TestValidate(dto);

        result.ShouldHaveValidationErrorFor(x => x.ConfirmationText);
    }

    [Fact]
    public void Validate_WhenValid_ThenHasNoValidationErrors()
    {
        var dto = new DeleteAccountDto
        {
            ConfirmationText = ConfirmationPhrase
        };

        var result = _validator.TestValidate(dto);

        result.ShouldNotHaveAnyValidationErrors();
    }
}