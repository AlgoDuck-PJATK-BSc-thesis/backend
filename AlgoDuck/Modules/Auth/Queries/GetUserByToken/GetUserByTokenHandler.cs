using System.Security.Claims;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace AlgoDuck.Modules.Auth.Queries.GetUserByToken;

public sealed class GetUserByTokenHandler : IGetUserByTokenHandler
{
    private readonly JwtTokenProvider _jwtTokenProvider;
    private readonly UserManager<ApplicationUser> _userManager;

    public GetUserByTokenHandler(
        JwtTokenProvider jwtTokenProvider,
        UserManager<ApplicationUser> userManager)
    {
        _jwtTokenProvider = jwtTokenProvider;
        _userManager = userManager;
    }

    public async Task<AuthUserDto?> HandleAsync(UserByTokenDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.AccessToken))
        {
            return null;
        }

        ClaimsPrincipal principal;

        try
        {
            principal = _jwtTokenProvider.ValidateToken(dto.AccessToken);
        }
        catch (SecurityTokenExpiredException)
        {
            return null;
        }
        catch
        {
            return null;
        }

        var userId = GetUserId(principal);
        if (userId == Guid.Empty)
        {
            return null;
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        return new AuthUserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed
        };
    }

    private static Guid GetUserId(ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier) ?? principal.FindFirst("sub");
        if (claim is null)
        {
            return Guid.Empty;
        }

        return Guid.TryParse(claim.Value, out var id) ? id : Guid.Empty;
    }
}