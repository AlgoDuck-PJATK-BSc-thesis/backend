using AlgoDuck.Modules.Auth.Commands.Session.RevokeOtherSessions;

namespace AlgoDuck.Tests.Modules.Auth.Commands.RevokeOtherSessions;

public sealed class RevokeOtherSessionsValidatorTests
{
    [Fact]
    public void Validate_WhenCurrentSessionIdEmpty_Fails()
    {
        var v = new RevokeOtherSessionsValidator();

        var r = v.Validate(new RevokeOtherSessionsDto { CurrentSessionId = Guid.Empty });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenCurrentSessionIdProvided_Passes()
    {
        var v = new RevokeOtherSessionsValidator();

        var r = v.Validate(new RevokeOtherSessionsDto { CurrentSessionId = Guid.NewGuid() });

        Assert.True(r.IsValid);
    }
}