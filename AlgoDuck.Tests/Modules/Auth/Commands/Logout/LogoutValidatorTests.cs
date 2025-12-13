using AlgoDuck.Modules.Auth.Commands.Logout;

namespace AlgoDuck.Tests.Modules.Auth.Commands.Logout;

public sealed class LogoutValidatorTests
{
    [Fact]
    public void Validate_AlwaysPasses()
    {
        var v = new LogoutValidator();

        var r = v.Validate(new LogoutDto { SessionId = null });

        Assert.True(r.IsValid);
    }
}