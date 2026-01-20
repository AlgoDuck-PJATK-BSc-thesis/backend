using AlgoDuck.Modules.Auth.Commands.Session.RevokeSession;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Session.RevokeSession;

public sealed class RevokeSessionValidatorTests
{
    [Fact]
    public void Validate_WhenSessionIdEmpty_Fails()
    {
        var v = new RevokeSessionValidator();

        var r = v.Validate(new RevokeSessionDto { SessionId = Guid.Empty });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenSessionIdProvided_Passes()
    {
        var v = new RevokeSessionValidator();

        var r = v.Validate(new RevokeSessionDto { SessionId = Guid.NewGuid() });

        Assert.True(r.IsValid);
    }
}