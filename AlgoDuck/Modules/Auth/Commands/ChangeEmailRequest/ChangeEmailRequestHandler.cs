using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailRequest;

public sealed class ChangeEmailRequestHandler : IChangeEmailRequestHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<ChangeEmailRequestDto> _validator;
    private readonly IConfiguration _configuration;

    public ChangeEmailRequestHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IValidator<ChangeEmailRequestDto> validator,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _validator = validator;
        _configuration = configuration;
    }

    public async Task HandleAsync(Guid userId, ChangeEmailRequestDto dto, CancellationToken cancellationToken)
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

        var existing = await _userManager.FindByEmailAsync(dto.NewEmail);
        if (existing is not null && existing.Id != user.Id)
        {
            throw new Shared.Exceptions.ValidationException("Email address is already in use.");
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, dto.NewEmail);
        var confirmationLink = BuildEmailChangeLink(user.Id, dto.NewEmail, token);

        await _emailSender.SendEmailChangeConfirmationAsync(user.Id, dto.NewEmail, confirmationLink, cancellationToken);
    }

    private string BuildEmailChangeLink(Guid userId, string newEmail, string token)
    {
        var frontendBaseUrl =
            _configuration["CORS:DevOrigins:0"] ??
            _configuration["CORS__DEVORIGINS__0"] ??
            "http://localhost:5173";

        var encodedToken = Uri.EscapeDataString(token);
        var encodedUserId = Uri.EscapeDataString(userId.ToString());
        var encodedEmail = Uri.EscapeDataString(newEmail);

        return $"{frontendBaseUrl}/auth/confirm-email-change?userId={encodedUserId}&email={encodedEmail}&token={encodedToken}";
    }
}