using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.DisableTwoFactor;

public sealed class DisableTwoFactorHandler : IDisableTwoFactorHandler
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DisableTwoFactorHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new PermissionException("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new PermissionException("User not found.");
        }

        if (!user.TwoFactorEnabled)
        {
            return;
        }

        user.TwoFactorEnabled = false;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new ValidationException($"Could not disable two-factor authentication: {errors}");
        }
    }
}