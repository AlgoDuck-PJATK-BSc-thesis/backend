using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Validators;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Validators;

public class EmailValidatorTests
{
    [Fact]
    public void Validate_WhenEmailIsNull_ThenThrowsValidationException()
    {
        var validator = new EmailValidator();

        Assert.Throws<ValidationException>(() => validator.Validate(null!));
    }

    [Fact]
    public void Validate_WhenEmailIsEmpty_ThenThrowsValidationException()
    {
        var validator = new EmailValidator();

        Assert.Throws<ValidationException>(() => validator.Validate(""));
    }

    [Fact]
    public void Validate_WhenEmailIsWhitespace_ThenThrowsValidationException()
    {
        var validator = new EmailValidator();

        Assert.Throws<ValidationException>(() => validator.Validate("   "));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("alice")]
    [InlineData("alice@")]
    [InlineData("@example.com")]
    [InlineData("alice@example")]
    [InlineData("alice@@example.com")]
    [InlineData("alice example.com")]
    public void Validate_WhenEmailHasInvalidFormat_ThenThrowsValidationException(string email)
    {
        var validator = new EmailValidator();

        Assert.Throws<ValidationException>(() => validator.Validate(email));
    }

    [Theory]
    [InlineData("alice@example.com")]
    [InlineData("ALICE@EXAMPLE.COM")]
    [InlineData("alice.smith+test@example.co.uk")]
    [InlineData("alice_smith@example.io")]
    public void Validate_WhenEmailIsValid_ThenDoesNotThrow(string email)
    {
        var validator = new EmailValidator();

        var exception = Record.Exception(() => validator.Validate(email));

        Assert.Null(exception);
    }
}