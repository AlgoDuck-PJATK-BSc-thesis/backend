using System.Security.Claims;
using AlgoDuck.Models;

namespace AlgoDuck.Modules.Auth.Interfaces;

public interface ITokenService
{
    Task<string> CreateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
