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
            return;
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
        var apiBaseUrl =
            _configuration["App:PublicApiUrl"] ??
            "http://localhost:8080";

        var frontendBaseUrl =
            _configuration["App:FrontendUrl"] ??
            _configuration["CORS:DevOrigins:0"] ??
            "http://localhost:5173";

        apiBaseUrl = apiBaseUrl.TrimEnd('/');
        frontendBaseUrl = frontendBaseUrl.TrimEnd('/');

        var encodedToken = Uri.EscapeDataString(token);
        var encodedUserId = Uri.EscapeDataString(userId.ToString());

        var returnUrl = $"{frontendBaseUrl}/auth/confirm-email?userId={encodedUserId}&token={encodedToken}";
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl);

        return $"{apiBaseUrl}/auth/email-verification?userId={encodedUserId}&token={encodedToken}&returnUrl={encodedReturnUrl}";
    }
}
