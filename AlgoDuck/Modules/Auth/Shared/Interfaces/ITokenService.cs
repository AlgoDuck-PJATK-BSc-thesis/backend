using AlgoDuck.Models;

namespace AlgoDuck.Modules.Auth.Shared.Interfaces;

public interface ITokenService
{
    Task<string> CreateAccessTokenAsync(ApplicationUser user);
    string GenerateRefreshToken();
    System.Security.Claims.ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}