using AlgoDuck.Modules.Auth.Commands.Password.RequestPasswordReset;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Password.RequestPasswordReset;

public sealed class RequestPasswordResetValidatorTests
{
    [Fact]
    public void Validate_WhenEmailEmpty_Fails()
    {
        var v = new RequestPasswordResetValidator();

        var r = v.Validate(new RequestPasswordResetDto { Email = "" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenEmailInvalid_Fails()
    {
        var v = new RequestPasswordResetValidator();

        var r = v.Validate(new RequestPasswordResetDto { Email = "not-an-email" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenEmailTooLong_Fails()
    {
        var v = new RequestPasswordResetValidator();
        var email = new string('a', 257);

        var r = v.Validate(new RequestPasswordResetDto { Email = email });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new RequestPasswordResetValidator();

        var r = v.Validate(new RequestPasswordResetDto { Email = "alice@example.com" });

        Assert.True(r.IsValid);
    }
}