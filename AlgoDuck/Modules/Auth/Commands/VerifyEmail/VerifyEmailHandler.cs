using System.Text;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

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
            var decoded = TryDecodeBase64Url(dto.Token);
            if (decoded is not null)
            {
                result = await _userManager.ConfirmEmailAsync(user, decoded);
            }
        }

        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new EmailVerificationException($"Email verification failed: {errors}");
        }
    }

    private static string? TryDecodeBase64Url(string token)
    {
        try
        {
            var bytes = WebEncoders.Base64UrlDecode(token);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }
}