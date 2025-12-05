using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.DisableTwoFactor;

public sealed class DisableTwoFactorHandler : IDisableTwoFactorHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<DisableTwoFactorDto> _validator;

    public DisableTwoFactorHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<DisableTwoFactorDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, DisableTwoFactorDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

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
            throw new Shared.Exceptions.ValidationException($"Could not disable two-factor authentication: {errors}");
        }
    }
}