using AlgoDuck.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.ResetPassword;

public sealed class ResetPasswordHandler : IResetPasswordHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<ResetPasswordDto> _validator;

    public ResetPasswordHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<ResetPasswordDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task HandleAsync(ResetPasswordDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null)
        {
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException("User not found.");

        }

        var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException($"Password reset failed: {errors}");
        }
    }
}