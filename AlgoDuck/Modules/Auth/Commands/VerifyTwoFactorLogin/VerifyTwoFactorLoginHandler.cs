using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.TwoFactor;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.VerifyTwoFactorLogin;

public sealed class VerifyTwoFactorLoginHandler : IVerifyTwoFactorLoginHandler
{
    private readonly ITwoFactorService _twoFactorService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly IValidator<VerifyTwoFactorLoginDto> _validator;

    public VerifyTwoFactorLoginHandler(
        ITwoFactorService twoFactorService,
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        IValidator<VerifyTwoFactorLoginDto> validator)
    {
        _twoFactorService = twoFactorService;
        _userManager = userManager;
        _tokenService = tokenService;
        _validator = validator;
    }

    public async Task<AuthResponse> HandleAsync(VerifyTwoFactorLoginDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var (ok, userId, error) = await _twoFactorService.VerifyLoginCodeAsync(dto.ChallengeId, dto.Code, cancellationToken);
        if (!ok)
        {
            throw new TwoFactorException(error ?? "Two-factor verification failed.");
        }

        if (userId == Guid.Empty)
        {
            throw new TwoFactorException("Invalid user identifier in two-factor challenge.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new TwoFactorException("User not found for two-factor login.");
        }

        if (!user.EmailConfirmed)
        {
            throw new EmailVerificationException("Email address is not confirmed.");
        }

        var authResponse = await _tokenService.GenerateAuthTokensAsync(user, cancellationToken);

        return authResponse;
    }
}