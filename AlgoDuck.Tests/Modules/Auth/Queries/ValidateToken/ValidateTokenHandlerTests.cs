using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlgoDuck.Modules.Auth.Queries.ValidateToken;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Tests.Modules.Auth.Queries.ValidateToken;

public sealed class ValidateTokenHandlerTests
{
    static JwtTokenProvider CreateProvider(string signingKey)
    {
        return new JwtTokenProvider(Options.Create(new JwtSettings
        {
            Issuer = "issuer",
            Audience = "audience",
            SigningKey = signingKey,
            AccessTokenMinutes = 15
        }));
    }

    static string CreateToken(string signingKey, Guid userId, Guid? sessionId, DateTimeOffset notBefore, DateTimeOffset expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "alice"),
            new(JwtRegisteredClaimNames.Email, "alice@example.com")
        };

        if (sessionId.HasValue)
        {
            claims.Add(new Claim("session_id", sessionId.Value.ToString()));
        }

        var jwt = new JwtSecurityToken(
            issuer: "issuer",
            audience: "audience",
            claims: claims,
            notBefore: notBefore.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }

    [Fact]
    public async Task HandleAsync_WhenAccessTokenEmpty_ReturnsInvalidNotExpired()
    {
        var handler = new ValidateTokenHandler(CreateProvider(new string('k', 64)));

        var result = await handler.HandleAsync(new ValidateTokenDto { AccessToken = "" }, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.False(result.IsExpired);
        Assert.Null(result.UserId);
        Assert.Null(result.SessionId);
        Assert.Null(result.ExpiresAt);
    }

    [Fact]
    public async Task HandleAsync_WhenSignatureInvalid_ReturnsInvalidNotExpired()
    {
        var handler = new ValidateTokenHandler(CreateProvider(new string('a', 64)));

        var token = CreateToken(new string('b', 64), Guid.NewGuid(), null, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));

        var result = await handler.HandleAsync(new ValidateTokenDto { AccessToken = token }, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.False(result.IsExpired);
    }

    [Fact]
    public async Task HandleAsync_WhenExpired_ReturnsInvalidExpired()
    {
        var signingKey = new string('k', 64);
        var handler = new ValidateTokenHandler(CreateProvider(signingKey));

        var token = CreateToken(signingKey, Guid.NewGuid(), null, DateTimeOffset.UtcNow.AddMinutes(-10), DateTimeOffset.UtcNow.AddMinutes(-5));

        var result = await handler.HandleAsync(new ValidateTokenDto { AccessToken = token }, CancellationToken.None);

        Assert.False(result.IsValid);
        Assert.True(result.IsExpired);
    }

    [Fact]
    public async Task HandleAsync_WhenValid_ReturnsValidAndExtractsClaims()
    {
        var signingKey = new string('k', 64);
        var handler = new ValidateTokenHandler(CreateProvider(signingKey));

        var userId = Guid.NewGuid();
        var sessionId = Guid.NewGuid();
        var token = CreateToken(signingKey, userId, sessionId, DateTimeOffset.UtcNow.AddMinutes(-1), DateTimeOffset.UtcNow.AddMinutes(10));

        var result = await handler.HandleAsync(new ValidateTokenDto { AccessToken = token }, CancellationToken.None);

        Assert.True(result.IsValid);
        Assert.False(result.IsExpired);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(sessionId, result.SessionId);
        Assert.NotNull(result.ExpiresAt);
        Assert.True(result.ExpiresAt!.Value > DateTimeOffset.UtcNow);
    }
}
