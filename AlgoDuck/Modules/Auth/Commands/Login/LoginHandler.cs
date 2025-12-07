using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.Login;

public sealed class LoginHandler : ILoginHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IValidator<LoginDto> _validator;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ITwoFactorService twoFactorService,
        IValidator<LoginDto> validator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
        _validator = validator;
    }

    public async Task<LoginResult> HandleAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await FindUserAsync(dto.UserNameOrEmail);
        if (user is null)
        {
            throw new Shared.Exceptions.ValidationException("Invalid username or password.");
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            throw new Shared.Exceptions.ValidationException("Invalid username or password.");
        }

        if (!user.EmailConfirmed)
        {
            throw new EmailVerificationException("Email address is not confirmed.");
        }

        if (user.TwoFactorEnabled)
        {
            var (challengeId, expiresAt) = await _twoFactorService.SendLoginCodeAsync(user, cancellationToken);

            return new LoginResult
            {
                TwoFactorRequired = true,
                Auth = null,
                ChallengeId = challengeId,
                ExpiresAt = expiresAt
            };
        }

        var authResponse = await _tokenService.GenerateAuthTokensAsync(user, cancellationToken);

        return new LoginResult
        {
            TwoFactorRequired = false,
            Auth = authResponse,
            ChallengeId = null,
            ExpiresAt = null
        };
    }

    private async Task<ApplicationUser?> FindUserAsync(string userNameOrEmail)
    {
        var byName = await _userManager.FindByNameAsync(userNameOrEmail);
        if (byName is not null)
        {
            return byName;
        }

        var byEmail = await _userManager.FindByEmailAsync(userNameOrEmail);
        return byEmail;
    }
}