using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.StartEmailVerification;

public sealed class StartEmailVerificationHandler : IStartEmailVerificationHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<StartEmailVerificationDto> _validator;
    private readonly IConfiguration _configuration;

    public StartEmailVerificationHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IValidator<StartEmailVerificationDto> validator,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _validator = validator;
        _configuration = configuration;
    }

    public async Task HandleAsync(StartEmailVerificationDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null)
        {
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException("Email address is not registered.");
        }

        if (user.EmailConfirmed)
        {
            return;
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = BuildEmailConfirmationLink(user.Id, token);

        await _emailSender.SendEmailConfirmationAsync(user.Id, user.Email!, confirmationLink, cancellationToken);
    }

    private string BuildEmailConfirmationLink(Guid userId, string token)
    {
        var frontendBaseUrl =
            _configuration["CORS:DevOrigins:0"] ??
            _configuration["CORS__DEVORIGINS__0"] ??
            "http://localhost:5173";

        var encodedToken = Uri.EscapeDataString(token);
        var encodedUserId = Uri.EscapeDataString(userId.ToString());

        return $"{frontendBaseUrl}/auth/confirm-email?userId={encodedUserId}&token={encodedToken}";
    }
}