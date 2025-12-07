using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.Auth.Commands.VerifyTwoFactorLogin;

public sealed class VerifyTwoFactorLoginHandler : IVerifyTwoFactorLoginHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ITwoFactorService _twoFactorService;
    private readonly IValidator<VerifyTwoFactorLoginDto> _validator;

    public VerifyTwoFactorLoginHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ITwoFactorService twoFactorService,
        IValidator<VerifyTwoFactorLoginDto> validator)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _twoFactorService = twoFactorService;
        _validator = validator;
    }

    public async Task<AuthResponse> HandleAsync(VerifyTwoFactorLoginDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var verification = await _twoFactorService.VerifyLoginCodeAsync(dto.ChallengeId, dto.Code, cancellationToken);
        if (!verification.ok)
        {
            throw new TwoFactorException(verification.error ?? "Invalid two-factor code.");
        }

        var user = await _userManager.FindByIdAsync(verification.userId.ToString());
        if (user is null)
        {
            throw new TwoFactorException("User not found for two-factor verification.");
        }

        if (!user.TwoFactorEnabled)
        {
            throw new TwoFactorException("Two-factor authentication is not enabled for this user.");
        }

        if (!user.EmailConfirmed)
        {
            throw new EmailVerificationException("Email address is not confirmed.");
        }

        var auth = await _tokenService.GenerateAuthTokensAsync(user, cancellationToken);
        return auth;
    }
}