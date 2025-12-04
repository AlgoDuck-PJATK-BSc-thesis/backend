using System.Security.Cryptography;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Shared.Utilities;

namespace AlgoDuck.Modules.Auth.Commands.RefreshToken;

public sealed class RefreshTokenHandler : IRefreshTokenHandler
{
    private readonly ApplicationCommandDbContext _commandDbContext;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwtSettings;

    public RefreshTokenHandler(
        ApplicationCommandDbContext commandDbContext,
        ITokenService tokenService,
        IOptions<JwtSettings> jwtOptions)
    {
        _commandDbContext = commandDbContext;
        _tokenService = tokenService;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<RefreshResult> HandleAsync(RefreshTokenDto dto, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            throw new ValidationException("Refresh token is required.");

        var rawRefresh = dto.RefreshToken;

        var prefixLength = Math.Min(rawRefresh.Length, 32);
        var clientPrefix = rawRefresh.Substring(0, prefixLength);

        var revoked = await _commandDbContext.Sessions
            .AsNoTracking()
            .Where(s => s.RevokedAtUtc != null && s.RefreshTokenPrefix == clientPrefix)
            .Select(s => new { s.SessionId, s.UserId, s.RefreshTokenHash, s.RefreshTokenSalt })
            .ToListAsync(cancellationToken);

        var reused = revoked.FirstOrDefault(s => MatchesRefresh(s.RefreshTokenHash, s.RefreshTokenSalt, rawRefresh));
        if (reused is not null)
        {
            var now = DateTime.UtcNow;

            var activeForUser = await _commandDbContext.Sessions
                .Where(x => x.UserId == reused.UserId && x.RevokedAtUtc == null)
                .ToListAsync(cancellationToken);

            foreach (var a in activeForUser)
                a.RevokedAtUtc = now;

            await _commandDbContext.SaveChangesAsync(cancellationToken);

            throw new UnauthorizedException("Refresh token reuse detected. All sessions revoked.");
        }

        var candidates = await _commandDbContext.Sessions
            .Include(s => s.User)
            .Where(s =>
                s.RevokedAtUtc == null &&
                s.ExpiresAtUtc > DateTime.UtcNow &&
                s.RefreshTokenPrefix == clientPrefix)
            .ToListAsync(cancellationToken);

        Session? session = null;
        foreach (var s in candidates)
        {
            if (MatchesRefresh(s.RefreshTokenHash, s.RefreshTokenSalt, rawRefresh))
            {
                session = s;
                break;
            }
        }

        if (session is null)
            throw new UnauthorizedException("Invalid refresh token.");

        var strategy = _commandDbContext.Database.CreateExecutionStrategy();

        RefreshResult result = new RefreshResult();

        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _commandDbContext.Database.BeginTransactionAsync(cancellationToken);

            session.RevokedAtUtc = DateTime.UtcNow;

            var newRaw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newSaltBytes = HashingHelper.GenerateSalt();
            var newHashB64 = HashingHelper.HashPassword(newRaw, newSaltBytes);
            var newSaltB64 = Convert.ToBase64String(newSaltBytes);

            var newPrefixLength = Math.Min(newRaw.Length, 32);
            var newRefreshPrefix = newRaw.Substring(0, newPrefixLength);

            var newId = Guid.NewGuid();
            var nowUtc = DateTime.UtcNow;
            var refreshExpiresUtc = nowUtc.AddDays(_jwtSettings.RefreshDays);

            var newSession = new Session
            {
                SessionId = newId,
                RefreshTokenHash = newHashB64,
                RefreshTokenSalt = newSaltB64,
                RefreshTokenPrefix = newRefreshPrefix,
                CreatedAtUtc = nowUtc,
                ExpiresAtUtc = refreshExpiresUtc,
                UserId = session.UserId,
                User = session.User
            };

            session.ReplacedBySessionId = newId;

            _commandDbContext.Sessions.Add(newSession);
            await _commandDbContext.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);

            var newAccessToken = await _tokenService.CreateAccessTokenAsync(session.User);

            var accessExpiresAt = nowUtc.AddMinutes(_jwtSettings.DurationInMinutes);

            result = new RefreshResult
            {
                AccessToken = newAccessToken,
                RefreshToken = newRaw,
                CsrfToken = string.Empty,
                AccessTokenExpiresAt = accessExpiresAt,
                RefreshTokenExpiresAt = refreshExpiresUtc,
                SessionId = newId,
                UserId = session.UserId
            };
        });

        return result;
    }

    private static bool MatchesRefresh(string storedHashB64, string storedSaltB64, string rawRefresh)
    {
        var salt = Convert.FromBase64String(storedSaltB64);
        var computedB64 = HashingHelper.HashPassword(rawRefresh, salt);
        return CryptographicOperations.FixedTimeEquals(
            Convert.FromBase64String(computedB64),
            Convert.FromBase64String(storedHashB64));
    }
}