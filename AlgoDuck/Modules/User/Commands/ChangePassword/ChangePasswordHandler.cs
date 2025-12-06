using AlgoDuck.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.User.Commands.ChangePassword;

public sealed class ChangePasswordHandler : IChangePasswordHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<ChangePasswordDto> _validator;

    public ChangePasswordHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<ChangePasswordDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (userId == Guid.Empty)
        {
            throw new Shared.Exceptions.ValidationException("User identifier is invalid.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new Shared.Exceptions.ValidationException("User not found.");
        }

        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Failed to change password.";
            throw new Shared.Exceptions.ValidationException(errorMessage);
        }
    }
}