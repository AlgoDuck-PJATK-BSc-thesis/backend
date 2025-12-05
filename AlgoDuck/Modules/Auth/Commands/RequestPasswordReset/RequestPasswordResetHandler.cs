using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.RequestPasswordReset;

public sealed class RequestPasswordResetHandler : IRequestPasswordResetHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<RequestPasswordResetDto> _validator;
    private readonly IConfiguration _configuration;

    public RequestPasswordResetHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IValidator<RequestPasswordResetDto> validator,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _validator = validator;
        _configuration = configuration;
    }

    public async Task HandleAsync(RequestPasswordResetDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user is null || !user.EmailConfirmed)
        {
            return;
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var resetLink = BuildPasswordResetLink(user.Id, token);

        await _emailSender.SendPasswordResetAsync(user.Id, user.Email!, resetLink, cancellationToken);
    }

    private string BuildPasswordResetLink(Guid userId, string token)
    {
        var frontendBaseUrl =
            _configuration["CORS:DevOrigins:0"] ??
            _configuration["CORS__DEVORIGINS__0"] ??
            "http://localhost:5173";

        var encodedToken = Uri.EscapeDataString(token);
        var encodedUserId = Uri.EscapeDataString(userId.ToString());

        return $"{frontendBaseUrl}/auth/reset-password?userId={encodedUserId}&token={encodedToken}";
    }
}