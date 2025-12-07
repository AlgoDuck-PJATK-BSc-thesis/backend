using System.Security.Cryptography;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Shared.Utilities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Auth.Commands.RefreshToken;

public sealed class RefreshTokenHandler : IRefreshTokenHandler
{
    private const int RefreshPrefixLength = 32;

    private readonly ApplicationCommandDbContext _commandDbContext;
    private readonly ITokenService _tokenService;
    private readonly IValidator<RefreshTokenDto> _validator;

    public RefreshTokenHandler(
        ApplicationCommandDbContext commandDbContext,
        ITokenService tokenService,
        IValidator<RefreshTokenDto> validator)
    {
        _commandDbContext = commandDbContext;
        _tokenService = tokenService;
        _validator = validator;
    }

    public async Task<RefreshResult> HandleAsync(RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var session = await FindSessionByRefreshTokenAsync(dto.RefreshToken, cancellationToken);
        if (session is null)
        {
            throw new TokenException("Invalid refresh token.");
        }

        var utcNow = DateTime.UtcNow;

        if (session.RevokedAtUtc.HasValue)
        {
            throw new TokenException("Refresh token has been revoked.");
        }

        if (session.ExpiresAtUtc <= utcNow)
        {
            session.RevokedAtUtc = session.RevokedAtUtc ?? utcNow;
            await _commandDbContext.SaveChangesAsync(cancellationToken);
            throw new TokenException("Refresh token has expired.");
        }

        return await _tokenService.RefreshTokensAsync(session, cancellationToken);
    }

    private async Task<Session?> FindSessionByRefreshTokenAsync(string rawRefreshToken, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(rawRefreshToken))
        {
            throw new TokenException("Refresh token is missing.");
        }

        var prefixLength = Math.Min(rawRefreshToken.Length, RefreshPrefixLength);
        if (prefixLength == 0)
        {
            throw new TokenException("Refresh token is invalid.");
        }

        var prefix = rawRefreshToken.Substring(0, prefixLength);

        var sessionsWithMatchingPrefix = await _commandDbContext.Sessions
            .Where(s => s.RefreshTokenPrefix == prefix)
            .ToListAsync(cancellationToken);

        if (sessionsWithMatchingPrefix.Count == 0)
        {
            return null;
        }

        foreach (var session in sessionsWithMatchingPrefix)
        {
            if (IsRefreshTokenMatch(rawRefreshToken, session))
            {
                return session;
            }
        }

        return null;
    }

    private static bool IsRefreshTokenMatch(string rawRefreshToken, Session session)
    {
        var saltBytes = Convert.FromBase64String(session.RefreshTokenSalt);
        var computedHash = HashingHelper.HashPassword(rawRefreshToken, saltBytes);
        var storedHashBytes = Convert.FromBase64String(session.RefreshTokenHash);
        var computedHashBytes = Convert.FromBase64String(computedHash);

        return CryptographicOperations.FixedTimeEquals(computedHashBytes, storedHashBytes);
    }
}