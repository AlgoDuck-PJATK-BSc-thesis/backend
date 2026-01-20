using AlgoDuck.Modules.Auth.Commands.Session.RefreshToken;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Commands.Session.RefreshToken;

public sealed class RefreshTokenValidatorTests
{
    [Fact]
    public void Validate_WhenRefreshTokenEmpty_Fails()
    {
        var v = new RefreshTokenValidator();

        var r = v.Validate(new RefreshTokenDto { RefreshToken = "" });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenRefreshTokenWhitespace_Fails()
    {
        var v = new RefreshTokenValidator();

        var r = v.Validate(new RefreshTokenDto { RefreshToken = "   " });

        Assert.False(r.IsValid);
    }

    [Fact]
    public void Validate_WhenRefreshTokenProvided_Passes()
    {
        var v = new RefreshTokenValidator();

        var r = v.Validate(new RefreshTokenDto { RefreshToken = "x" });

        Assert.True(r.IsValid);
    }
}