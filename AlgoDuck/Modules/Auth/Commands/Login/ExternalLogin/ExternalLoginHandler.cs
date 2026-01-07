using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Utils;
using AlgoDuck.Shared.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.Login.ExternalLogin;

public sealed class ExternalLoginHandler : IExternalLoginHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IValidator<ExternalLoginDto> _validator;
    private readonly IDefaultDuckService _defaultDuckService;

    public ExternalLoginHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IValidator<ExternalLoginDto> validator,
        IDefaultDuckService defaultDuckService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _validator = validator;
        _defaultDuckService = defaultDuckService;
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
            var username = await UsernameGenerator.GenerateUniqueDictionaryWithNumbersAsync(_userManager, cancellationToken);

            user = new ApplicationUser
            {
                UserName = username,
                Email = dto.Email,
                EmailConfirmed = true
            };

            for (var attempt = 0; attempt < 8; attempt++)
            {
                var createResult = await _userManager.CreateAsync(user);
                if (createResult.Succeeded) break;

                var isDuplicate = createResult.Errors.Any(e => string.Equals(e.Code, "DuplicateUserName", StringComparison.OrdinalIgnoreCase));
                if (isDuplicate && attempt < 7)
                {
                    user.UserName = await UsernameGenerator.GenerateUniqueDictionaryWithNumbersAsync(_userManager, cancellationToken);
                    continue;
                }

                var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
                throw new Shared.Exceptions.ValidationException($"Could not create user from external login: {errors}");
            }
        }
        else
        {
            if (!user.EmailConfirmed)
            {
                user.EmailConfirmed = true;
            }

            if (NeedsUsernameMigration(user, dto.Email))
            {
                for (var attempt = 0; attempt < 8; attempt++)
                {
                    user.UserName = await UsernameGenerator.GenerateUniqueDictionaryWithNumbersAsync(_userManager, cancellationToken);

                    var updateResult = await _userManager.UpdateAsync(user);
                    if (updateResult.Succeeded) break;

                    var isDuplicate = updateResult.Errors.Any(e => string.Equals(e.Code, "DuplicateUserName", StringComparison.OrdinalIgnoreCase));
                    if (isDuplicate && attempt < 7) continue;

                    var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    throw new Shared.Exceptions.ValidationException($"Could not update username for external login: {errors}");
                }
            }
            else
            {
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                    throw new Shared.Exceptions.ValidationException($"Could not update user email confirmation: {errors}");
                }
            }
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, "user");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                throw new Shared.Exceptions.ValidationException($"Failed to assign default role: {errors}");
            }
        }

        await _defaultDuckService.EnsureAlgoduckOwnedAndSelectedAsync(user.Id, cancellationToken);

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

    static bool NeedsUsernameMigration(ApplicationUser user, string email)
    {
        var uname = user.UserName ?? string.Empty;
        if (uname.Contains('@')) return true;
        if (string.Equals(uname, email, StringComparison.OrdinalIgnoreCase)) return true;
        return false;
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

        if (normalized is "microsoft" or "microsoft-oauth")
        {
            return "Microsoft";
        }

        return null;
    }
}
