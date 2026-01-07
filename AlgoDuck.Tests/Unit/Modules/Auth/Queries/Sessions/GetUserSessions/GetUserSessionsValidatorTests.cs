using AlgoDuck.Modules.Auth.Queries.Sessions.GetUserSessions;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Queries.Sessions.GetUserSessions;

public sealed class GetUserSessionsValidatorTests
{
    [Fact]
    public void Validate_WhenEmpty_Fails()
    {
        var validator = new GetUserSessionsValidator();

        var result = validator.Validate(Guid.Empty);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenNonEmpty_Passes()
    {
        var validator = new GetUserSessionsValidator();

        var result = validator.Validate(Guid.NewGuid());

        Assert.True(result.IsValid);
    }
}