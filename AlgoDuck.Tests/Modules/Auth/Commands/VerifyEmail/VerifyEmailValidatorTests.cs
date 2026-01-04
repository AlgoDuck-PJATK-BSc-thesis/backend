using AlgoDuck.Modules.Auth.Commands.Email.VerifyEmail;

namespace AlgoDuck.Tests.Modules.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailValidatorTests
{
    [Fact]
    public void Validate_WhenUserIdEmpty_Fails()
    {
        var v = new VerifyEmailValidator();

        var r = v.Validate(new VerifyEmailDto { UserId = Guid.Empty, Token = "t" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenTokenEmpty_Fails()
    {
        var v = new VerifyEmailValidator();

        var r = v.Validate(new VerifyEmailDto { UserId = Guid.NewGuid(), Token = "" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new VerifyEmailValidator();

        var r = v.Validate(new VerifyEmailDto { UserId = Guid.NewGuid(), Token = "t" });

        Assert.True(r.IsValid);
    }
}