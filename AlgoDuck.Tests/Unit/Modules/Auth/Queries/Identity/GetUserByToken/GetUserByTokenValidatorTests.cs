using AlgoDuck.Modules.Auth.Queries.Identity.GetUserByToken;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Queries.Identity.GetUserByToken;

public sealed class GetUserByTokenValidatorTests
{
    [Fact]
    public void Validate_WhenAccessTokenEmpty_Fails()
    {
        var validator = new GetUserByTokenValidator();

        var result = validator.Validate(new UserByTokenDto { AccessToken = "" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAccessTokenProvided_Passes()
    {
        var validator = new GetUserByTokenValidator();

        var result = validator.Validate(new UserByTokenDto { AccessToken = "x" });

        Assert.True(result.IsValid);
    }
}