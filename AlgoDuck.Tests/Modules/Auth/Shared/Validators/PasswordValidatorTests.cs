using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Validators;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Validators;

public class PasswordValidatorTests
{
    [Fact]
    public void Validate_WhenPasswordIsNull_ThenThrowsValidationException()
    {
        var validator = new PasswordValidator();

        Assert.Throws<ValidationException>(() => validator.Validate(null!));
    }

    [Fact]
    public void Validate_WhenPasswordIsEmpty_ThenThrowsValidationException()
    {
        var validator = new PasswordValidator();

        Assert.Throws<ValidationException>(() => validator.Validate(""));
    }

    [Fact]
    public void Validate_WhenPasswordIsWhitespace_ThenThrowsValidationException()
    {
        var validator = new PasswordValidator();

        Assert.Throws<ValidationException>(() => validator.Validate("   "));
    }

    [Fact]
    public void Validate_WhenPasswordTooShort_ThenThrowsValidationException()
    {
        var validator = new PasswordValidator();
        var password = "Abc123";

        Assert.Throws<ValidationException>(() => validator.Validate(password));
    }

    [Fact]
    public void Validate_WhenPasswordTooLong_ThenThrowsValidationException()
    {
        var validator = new PasswordValidator();
        var password = new string('A', 200) + "1a";

        Assert.Throws<ValidationException>(() => validator.Validate(password));
    }

    [Fact]
    public void Validate_WhenPasswordMissingUppercase_ThenThrowsValidationException()
    {
        var validator = new PasswordValidator();
        var password = "password123";

        Assert.Throws<ValidationException>(() => validator.Validate(password));
    }

    [Fact]
    public void Validate_WhenPasswordMissingLowercase_ThenThrowsValidationException()
    {
        var validator = new PasswordValidator();
        var password = "PASSWORD123";

        Assert.Throws<ValidationException>(() => validator.Validate(password));
    }

    [Fact]
    public void Validate_WhenPasswordMissingDigit_ThenThrowsValidationException()
    {
        var validator = new PasswordValidator();
        var password = "PasswordOnly";

        Assert.Throws<ValidationException>(() => validator.Validate(password));
    }

    [Fact]
    public void Validate_WhenPasswordMeetsAllRequirements_ThenDoesNotThrow()
    {
        var validator = new PasswordValidator();
        var password = "StrongPassw0rd";

        var exception = Record.Exception(() => validator.Validate(password));

        Assert.Null(exception);
    }
}
