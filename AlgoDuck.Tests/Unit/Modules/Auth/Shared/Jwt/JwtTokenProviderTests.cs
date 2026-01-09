using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Jwt;

public sealed class JwtTokenProviderTests
{
    static JwtSettings CreateSettings(int accessTokenMinutes = 15)
    {
        return new JwtSettings
        {
            Issuer = "issuer",
            Audience = "audience",
            SigningKey = new string('k', 64),
            AccessTokenMinutes = accessTokenMinutes
        };
    }

    static string CreateToken(JwtSettings settings, ApplicationUser user, DateTimeOffset notBefore, DateTimeOffset expiresAt, string algorithm)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));
        var creds = new SigningCredentials(key, algorithm);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: settings.Issuer,
            audience: settings.Audience,
            claims: claims,
            notBefore: notBefore.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    [Fact]
    public void CreateAccessToken_ContainsExpectedClaims_AndHasExpiryInFuture()
    {
        var provider = new JwtTokenProvider(Options.Create(CreateSettings()));
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            Email = "alice@example.com"
        };

        var sessionId = Guid.NewGuid();
        var token = provider.CreateAccessToken(user, sessionId, out var expiresAt);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAt > DateTimeOffset.UtcNow);

        var principal = provider.ValidateToken(token);

        Assert.Equal(user.Id.ToString(), principal.FindFirstValue(ClaimTypes.NameIdentifier));
        Assert.Equal(user.UserName, principal.FindFirstValue(ClaimTypes.Name));
        Assert.Equal(user.Email, principal.FindFirstValue(ClaimTypes.Email));
        Assert.Equal(sessionId.ToString(), principal.FindFirstValue("sid"));
    }

    [Fact]
    public void ValidateToken_WhenTokenSignedWithDifferentKey_Throws()
    {
        var providerA = new JwtTokenProvider(Options.Create(new JwtSettings
        {
            Issuer = "issuer",
            Audience = "audience",
            SigningKey = new string('a', 64),
            AccessTokenMinutes = 15
        }));

        var providerB = new JwtTokenProvider(Options.Create(new JwtSettings
        {
            Issuer = "issuer",
            Audience = "audience",
            SigningKey = new string('b', 64),
            AccessTokenMinutes = 15
        }));

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@example.com" };
        var token = providerA.CreateAccessToken(user, Guid.NewGuid(), out _);

        Assert.ThrowsAny<SecurityTokenException>(() => providerB.ValidateToken(token));
    }

    [Fact]
    public void ValidateToken_WhenExpired_Throws()
    {
        var settings = CreateSettings();
        var provider = new JwtTokenProvider(Options.Create(settings));
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@example.com" };

        var now = DateTimeOffset.UtcNow;
        var token = CreateToken(
            settings,
            user,
            notBefore: now.AddMinutes(-10),
            expiresAt: now.AddMinutes(-5),
            algorithm: SecurityAlgorithms.HmacSha256
        );

        Assert.Throws<SecurityTokenExpiredException>(() => provider.ValidateToken(token));
    }
}
