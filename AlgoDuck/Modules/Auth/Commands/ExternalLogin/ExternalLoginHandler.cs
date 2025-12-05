using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.ExternalLogin;

public sealed class ExternalLoginHandler : IExternalLoginHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IValidator<ExternalLoginDto> _validator;

    public ExternalLoginHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IValidator<ExternalLoginDto> validator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _validator = validator;
    }

    public async Task<AuthResponse> HandleAsync(ExternalLoginDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var provider = NormalizeProvider(dto.Provider);
        if (provider is null)
        {
            throw new Shared.Exceptions.ValidationException("Unsupported external provider.");
        }

        var user = await _userManager.FindByEmailAsync(dto.Email);

        if (user is null)
        {
            user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new Shared.Exceptions.ValidationException($"Could not create user from external login: {errors}");
            }
        }
        else if (!user.EmailConfirmed)
        {
            user.EmailConfirmed = true;
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                throw new Shared.Exceptions.ValidationException($"Could not update user email confirmation: {errors}");
            }
        }

        var existingByLogin = await _userManager.FindByLoginAsync(provider, dto.ExternalUserId);
        if (existingByLogin is not null && existingByLogin.Id != user.Id)
        {
            throw new Shared.Exceptions.ValidationException("External login is already linked to another account.");
        }

        var currentLogins = await _userManager.GetLoginsAsync(user);
        var alreadyLinked = currentLogins.Any(l => l.LoginProvider == provider && l.ProviderKey == dto.ExternalUserId);

        if (!alreadyLinked)
        {
            var info = new UserLoginInfo(provider, dto.ExternalUserId, provider);
            var addLoginResult = await _userManager.AddLoginAsync(user, info);
            if (!addLoginResult.Succeeded)
            {
                var errors = string.Join("; ", addLoginResult.Errors.Select(e => e.Description));
                throw new Shared.Exceptions.ValidationException($"Could not link external login: {errors}");
            }
        }

        var authResponse = await _tokenService.GenerateAuthTokensAsync(user, cancellationToken);

        return authResponse;
    }

    private static string? NormalizeProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return null;
        }

        var normalized = provider.Trim().ToLowerInvariant();

        if (normalized is "google" or "google-oauth")
        {
            return "Google";
        }

        if (normalized is "github" or "github-oauth")
        {
            return "GitHub";
        }

        if (normalized is "facebook" or "facebook-oauth")
        {
            return "Facebook";
        }

        return null;
    }
}