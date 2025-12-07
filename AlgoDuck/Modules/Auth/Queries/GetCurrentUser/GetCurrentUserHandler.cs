using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Queries.GetCurrentUser;

public sealed class GetCurrentUserHandler : IGetCurrentUserHandler
{
    private readonly UserManager<ApplicationUser> _userManager;

    public GetCurrentUserHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<AuthUserDto?> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
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
}