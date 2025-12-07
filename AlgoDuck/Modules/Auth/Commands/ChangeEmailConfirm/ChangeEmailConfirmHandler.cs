using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailConfirm;

public sealed class ChangeEmailConfirmHandler : IChangeEmailConfirmHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<ChangeEmailConfirmDto> _validator;

    public ChangeEmailConfirmHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<ChangeEmailConfirmDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task HandleAsync(ChangeEmailConfirmDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null)
        {
            throw new EmailVerificationException("User not found.");
        }

        var result = await _userManager.ChangeEmailAsync(user, dto.NewEmail, dto.Token);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new EmailVerificationException($"Email change verification failed: {errors}");
        }

        user.EmailConfirmed = true;
        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
        {
            var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
            throw new EmailVerificationException($"Failed to finalize email change: {errors}");
        }
    }
}