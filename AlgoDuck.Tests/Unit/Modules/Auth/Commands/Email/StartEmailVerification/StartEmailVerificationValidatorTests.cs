using AlgoDuck.Modules.Auth.Commands.Email.StartEmailVerification;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Email.StartEmailVerification;

public sealed class StartEmailVerificationValidatorTests
{
    [Fact]
    public void Validate_WhenEmailEmpty_Fails()
    {
        var v = new StartEmailVerificationValidator();

        var r = v.Validate(new StartEmailVerificationDto { Email = "" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenEmailInvalid_Fails()
    {
        var v = new StartEmailVerificationValidator();

        var r = v.Validate(new StartEmailVerificationDto { Email = "not-an-email" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenValid_Passes()
    {
        var v = new StartEmailVerificationValidator();

        var r = v.Validate(new StartEmailVerificationDto { Email = "alice@example.com" });

        Assert.True(r.IsValid);
    }
}