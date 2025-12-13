using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Tests.Modules.Auth.Shared.Jwt;

public sealed class TokenRefreshServiceTests
{
    static JwtSettings CreateSettings(int accessMinutes = 15, int refreshMinutes = 60)
    {
        return new JwtSettings
        {
            Issuer = "issuer",
            Audience = "audience",
            SigningKey = new string('k', 64),
            AccessTokenMinutes = accessMinutes,
            RefreshTokenMinutes = refreshMinutes
        };
    }

    [Fact]
    public async Task CreateRefreshResultAsync_ReturnsTokensAndExpiryAndIds()
    {
        var settings = CreateSettings(accessMinutes: 10, refreshMinutes: 120);
        var jwtProvider = new JwtTokenProvider(Options.Create(settings));
        var tokenGenerator = new TokenGenerator();
        var service = new TokenRefreshService(jwtProvider, tokenGenerator, Options.Create(settings));

        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            Email = "alice@example.com"
        };

        var sessionId = Guid.NewGuid();

        var result = await service.CreateRefreshResultAsync(user, sessionId, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(result.AccessToken));
        Assert.False(string.IsNullOrWhiteSpace(result.RefreshToken));
        Assert.False(string.IsNullOrWhiteSpace(result.CsrfToken));

        Assert.Equal(sessionId, result.SessionId);
        Assert.Equal(user.Id, result.UserId);

        Assert.True(result.AccessTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(9));
        Assert.True(result.AccessTokenExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(10).AddSeconds(5));

        Assert.True(result.RefreshTokenExpiresAt > DateTimeOffset.UtcNow.AddMinutes(119));
        Assert.True(result.RefreshTokenExpiresAt <= DateTimeOffset.UtcNow.AddMinutes(120).AddSeconds(5));
    }
}