using AlgoDuck.Modules.Auth.Commands.DisableTwoFactor;

namespace AlgoDuck.Tests.Modules.Auth.Commands.DisableTwoFactor;

public sealed class DisableTwoFactorValidatorTests
{
    [Fact]
    public void Validate_AlwaysPasses()
    {
        var v = new DisableTwoFactorValidator();

        var r = v.Validate(new DisableTwoFactorDto());

        Assert.True(r.IsValid);
    }
}