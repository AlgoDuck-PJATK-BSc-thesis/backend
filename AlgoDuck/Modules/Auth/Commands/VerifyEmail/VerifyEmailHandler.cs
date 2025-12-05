using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailHandler : IVerifyEmailHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<VerifyEmailDto> _validator;

    public VerifyEmailHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<VerifyEmailDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task HandleAsync(VerifyEmailDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _userManager.FindByIdAsync(dto.UserId.ToString());
        if (user is null)
        {
            throw new EmailVerificationException("User not found.");
        }

        if (user.EmailConfirmed)
        {
            return;
        }

        var result = await _userManager.ConfirmEmailAsync(user, dto.Token);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new EmailVerificationException($"Email verification failed: {errors}");
        }
    }
}