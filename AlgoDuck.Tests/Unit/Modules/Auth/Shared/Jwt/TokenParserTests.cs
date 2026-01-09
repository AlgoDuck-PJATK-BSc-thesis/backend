using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Tests.Unit.Modules.Auth.Shared.Jwt;

public sealed class TokenParserTests
{
    static JwtSettings CreateSettings()
    {
        return new JwtSettings
        {
            Issuer = "issuer",
            Audience = "audience",
            SigningKey = new string('k', 64),
            AccessTokenMinutes = 15
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
    public void GetPrincipalFromExpiredToken_WhenTokenIsValid_ReturnsPrincipal()
    {
        var settings = CreateSettings();
        var provider = new JwtTokenProvider(Options.Create(settings));
        var parser = new TokenParser(Options.Create(settings));
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@example.com" };

        var token = provider.CreateAccessToken(user, Guid.NewGuid(), out _);

        var principal = parser.GetPrincipalFromExpiredToken(token);

        Assert.NotNull(principal);
        Assert.Equal(user.Id.ToString(), principal.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WhenTokenHasWrongKey_Throws()
    {
        var settingsA = new JwtSettings { Issuer = "issuer", Audience = "audience", SigningKey = new string('a', 64) };
        var settingsB = new JwtSettings { Issuer = "issuer", Audience = "audience", SigningKey = new string('b', 64) };

        var provider = new JwtTokenProvider(Options.Create(settingsA));
        var parser = new TokenParser(Options.Create(settingsB));

        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@example.com" };
        var token = provider.CreateAccessToken(user, Guid.NewGuid(), out _);

        Assert.ThrowsAny<SecurityTokenException>(() => parser.GetPrincipalFromExpiredToken(token));
    }

    [Fact]
    public void GetPrincipalFromExpiredToken_WhenAlgorithmIsNotExpected_ThrowsSecurityTokenException()
    {
        var settings = CreateSettings();
        var parser = new TokenParser(Options.Create(settings));
        var user = new ApplicationUser { Id = Guid.NewGuid(), UserName = "alice", Email = "alice@example.com" };

        var now = DateTimeOffset.UtcNow;
        var token = CreateToken(
            settings,
            user,
            notBefore: now.AddMinutes(-1),
            expiresAt: now.AddMinutes(10),
            algorithm: SecurityAlgorithms.HmacSha512
        );

        var ex = Assert.Throws<SecurityTokenException>(() => parser.GetPrincipalFromExpiredToken(token));
        Assert.Equal("Invalid token", ex.Message);
    }
}
