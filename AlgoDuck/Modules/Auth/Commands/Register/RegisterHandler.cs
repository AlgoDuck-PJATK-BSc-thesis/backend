using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.Register;

public sealed class RegisterHandler : IRegisterHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<RegisterDto> _validator;
    private readonly IConfiguration _configuration;

    public RegisterHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IValidator<RegisterDto> validator,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _validator = validator;
        _configuration = configuration;
    }

    public async Task<AuthUserDto> HandleAsync(RegisterDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var existingByUserName = await _userManager.FindByNameAsync(dto.UserName);
        if (existingByUserName is not null)
        {
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException("Username is already taken.");
        }

        var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingByEmail is not null)
        {
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException($"User registration failed: {errors}");
        }

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationLink = BuildEmailConfirmationLink(user.Id, token);

        await _emailSender.SendEmailConfirmationAsync(user.Id, user.Email!, confirmationLink, cancellationToken);

        return new AuthUserDto
        {
            Id = user.Id,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            EmailConfirmed = user.EmailConfirmed
        };
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