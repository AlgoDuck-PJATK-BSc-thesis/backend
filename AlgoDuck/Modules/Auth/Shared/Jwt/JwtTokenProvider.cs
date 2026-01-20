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

        var key = string.IsNullOrWhiteSpace(_settings.SigningKey)
            ? "dev_signing_key_dev_signing_key_dev_signing_key_32+"
            : _settings.SigningKey;

        _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        {
            KeyId = "algoduck-symmetric-v1"
        };

        _signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
    }

    public string CreateAccessToken(ApplicationUser user, Guid sessionId, out DateTimeOffset expiresAt, IEnumerable<string>? roles = null)
    {
        var now = DateTimeOffset.UtcNow;
        expiresAt = now.AddMinutes(_settings.AccessTokenMinutes);

        var email = user.Email ?? string.Empty;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, email),
            new(ClaimTypes.Email, email),
            new("sid", sessionId.ToString())
        };

        if (roles is not null)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var raw in roles)
            {
                var role = raw.Trim();
                if (string.IsNullOrWhiteSpace(role)) continue;
                if (!seen.Add(role)) continue;

                claims.Add(new Claim(ClaimTypes.Role, role));
                claims.Add(new Claim("role", role));
            }
        }

        var token = new JwtSecurityToken(
            issuer: string.IsNullOrWhiteSpace(_settings.Issuer) ? null : _settings.Issuer,
            audience: string.IsNullOrWhiteSpace(_settings.Audience) ? null : _settings.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: expiresAt.UtcDateTime,
            signingCredentials: _signingCredentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidateToken(string token)
    {
        var handler = new JwtSecurityTokenHandler
        {
            MapInboundClaims = false
        };

        var parameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = _signingKey,
            ValidateIssuer = !string.IsNullOrWhiteSpace(_settings.Issuer),
            ValidIssuer = string.IsNullOrWhiteSpace(_settings.Issuer) ? null : _settings.Issuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(_settings.Audience),
            ValidAudience = string.IsNullOrWhiteSpace(_settings.Audience) ? null : _settings.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        return handler.ValidateToken(token, parameters, out _);
    }
}
