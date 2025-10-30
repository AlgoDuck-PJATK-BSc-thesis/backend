using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AlgoDuck.Models.User;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Modules.Auth.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwt;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenService(IOptions<JwtSettings> options, UserManager<ApplicationUser> userManager)
        {
            _jwt = options.Value;
            _userManager = userManager;
        }

        public async Task<string> CreateAccessTokenAsync(ApplicationUser user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var r in roles)
                claims.Add(new Claim(ClaimTypes.Role, r));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwt.DurationInMinutes),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)),
                ValidateIssuer = _jwt.ValidateIssuer,
                ValidateAudience = _jwt.ValidateAudience,
                ValidateLifetime = false,
                ValidIssuer = _jwt.Issuer,
                ValidAudience = _jwt.Audience,
                ClockSkew = TimeSpan.FromSeconds(_jwt.ClockSkewSeconds)
            };

            var handler = new JwtSecurityTokenHandler();
            try
            {
                return handler.ValidateToken(token, parameters, out _);
            }
            catch (Exception ex)
            {
                throw new InvalidTokenException("Invalid or malformed token.", ex);
            }
        }
    }
}