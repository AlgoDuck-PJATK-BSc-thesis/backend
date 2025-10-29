using System.Security.Claims;
using AlgoDuck.Models.User;

namespace AlgoDuck.Modules.Auth.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(ApplicationUser user);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}
