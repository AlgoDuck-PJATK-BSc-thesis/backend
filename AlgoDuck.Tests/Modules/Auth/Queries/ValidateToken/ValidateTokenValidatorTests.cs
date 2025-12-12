using AlgoDuck.Modules.Auth.Queries.ValidateToken;

namespace AlgoDuck.Tests.Modules.Auth.Queries.ValidateToken;

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