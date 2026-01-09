using AlgoDuck.Modules.Auth.Queries.Token.ValidateToken;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Queries.Token.ValidateToken;

public sealed class ValidateTokenValidatorTests
{
    [Fact]
    public void Validate_WhenAccessTokenEmpty_Fails()
    {
        var validator = new ValidateTokenValidator();

        var result = validator.Validate(new ValidateTokenDto { AccessToken = "" });

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Validate_WhenAccessTokenProvided_Passes()
    {
        var validator = new ValidateTokenValidator();

        var result = validator.Validate(new ValidateTokenDto { AccessToken = "x" });

        Assert.True(result.IsValid);
    }
}