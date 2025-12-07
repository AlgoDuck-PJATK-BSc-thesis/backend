using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Modules.Auth.Shared.Jwt;

public sealed class TokenParser
{
    private readonly JwtSettings _settings;
    private readonly TokenValidationParameters _validationParameters;

    public TokenParser(IOptions<JwtSettings> options)
    {
        _settings = options.Value;
        _validationParameters = BuildValidationParameters(_settings);
    }

    private static TokenValidationParameters BuildValidationParameters(JwtSettings settings)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey));

        return new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = settings.Issuer,
            ValidateAudience = true,
            ValidAudience = settings.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };
    }

    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(token, _validationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwt ||
            !jwt.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return principal;
    }
}