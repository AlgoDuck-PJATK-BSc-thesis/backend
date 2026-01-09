using AlgoDuck.Modules.Auth.Shared.Jwt;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Jwt;

public sealed class JwtSettingsTests
{
    [Fact]
    public void Defaults_AreAsExpected()
    {
        var settings = new JwtSettings();

        Assert.Equal(string.Empty, settings.Issuer);
        Assert.Equal(string.Empty, settings.Audience);
        Assert.Equal(string.Empty, settings.SigningKey);

        Assert.Equal(15, settings.AccessTokenMinutes);
        Assert.Equal(60 * 24 * 7, settings.RefreshTokenMinutes);

        Assert.Equal("jwt", settings.AccessTokenCookieName);
        Assert.Equal("refresh_token", settings.RefreshTokenCookieName);
        Assert.Equal("csrf_token", settings.CsrfCookieName);
        Assert.Equal("X-CSRF-Token", settings.CsrfHeaderName);

        Assert.Null(settings.CookieDomain);
    }
}