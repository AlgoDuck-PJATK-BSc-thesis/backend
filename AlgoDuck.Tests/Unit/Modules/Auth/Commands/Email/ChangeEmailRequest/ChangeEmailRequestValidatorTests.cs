using AlgoDuck.Modules.Auth.Commands.Email.ChangeEmailRequest;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Email.ChangeEmailRequest;

public sealed class ChangeEmailRequestValidatorTests
{
    [Fact]
    public void Validate_WhenNewEmailEmpty_Fails()
    {
        var v = new ChangeEmailRequestValidator();

        var r = v.Validate(new ChangeEmailRequestDto { NewEmail = "" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenNewEmailInvalid_Fails()
    {
        var v = new ChangeEmailRequestValidator();

        var r = v.Validate(new ChangeEmailRequestDto { NewEmail = "not-an-email" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new ChangeEmailRequestValidator();

        var r = v.Validate(new ChangeEmailRequestDto { NewEmail = "alice@example.com" });

        Assert.True(r.IsValid);
    }
}