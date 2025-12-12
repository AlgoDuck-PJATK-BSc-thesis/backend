using AlgoDuck.Modules.Auth.Commands.EnableTwoFactor;

namespace AlgoDuck.Tests.Modules.Auth.Commands.EnableTwoFactor;

public sealed class EnableTwoFactorValidatorTests
{
    [Fact]
    public void Validate_AlwaysPasses()
    {
        var v = new EnableTwoFactorValidator();

        var r = v.Validate(new EnableTwoFactorDto());

        Assert.True(r.IsValid);
    }
}