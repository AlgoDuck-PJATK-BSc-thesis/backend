using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AlgoDuck.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Modules.Auth.Shared.Jwt;

public sealed class JwtTokenProvider
{
    private readonly JwtSettings _settings;
    private readonly SymmetricSecurityKey _signingKey;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenProvider(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SigningKey));
        _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
    }

    public string CreateAccessToken(ApplicationUser user, out DateTimeOffset expiresAt)
    {
        var now = DateTimeOffset.UtcNow;
        expiresAt = now.AddMinutes(_settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: _signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}