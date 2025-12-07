using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Modules.Auth.Shared.Jwt;
using AlgoDuck.Modules.Auth.Shared.Mappers;
using AlgoDuck.Shared.Utilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class TokenService : ITokenService
{
    private const int RefreshPrefixLength = 32;

    private readonly ISessionRepository _sessionRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly ApplicationCommandDbContext _commandDbContext;
    private readonly JwtTokenProvider _jwtTokenProvider;
    private readonly JwtSettings _settings;

    public TokenService(
        ISessionRepository sessionRepository,
        ITokenRepository tokenRepository,
        ApplicationCommandDbContext commandDbContext,
        JwtTokenProvider jwtTokenProvider,
        IOptions<JwtSettings> options)
    {
        _sessionRepository = sessionRepository;
        _tokenRepository = tokenRepository;
        _commandDbContext = commandDbContext;
        _jwtTokenProvider = jwtTokenProvider;
        _settings = options.Value;
    }

    public async Task<AuthResponse> GenerateAuthTokensAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenProvider.CreateAccessToken(user, out var accessExpiresAt);

        var rawRefresh = GenerateRefreshToken();
        var saltBytes = HashingHelper.GenerateSalt();
        var hashB64 = HashingHelper.HashPassword(rawRefresh, saltBytes);
        var saltB64 = Convert.ToBase64String(saltBytes);

        var prefixLength = Math.Min(rawRefresh.Length, RefreshPrefixLength);
        var refreshPrefix = rawRefresh.Substring(0, prefixLength);

        var now = DateTimeOffset.UtcNow;
        var refreshExpires = now.AddMinutes(_settings.RefreshTokenMinutes);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            RefreshTokenHash = hashB64,
            RefreshTokenSalt = saltB64,
            RefreshTokenPrefix = refreshPrefix,
            CreatedAtUtc = now.UtcDateTime,
            ExpiresAtUtc = refreshExpires.UtcDateTime,
            UserId = user.Id,
            User = user
        };

        await _sessionRepository.AddAsync(session, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        return AuthMapper.ToAuthResponse(
            user,
            session,
            accessToken,
            rawRefresh,
            GenerateCsrfToken(),
            accessExpiresAt,
            refreshExpires);
    }

    public async Task<RefreshResult> RefreshTokensAsync(Session session, CancellationToken cancellationToken)
    {
        if (session.RevokedAtUtc.HasValue)
        {
            throw new TokenException("Session has been revoked.");
        }

        if (session.ExpiresAtUtc <= DateTime.UtcNow)
        {
            throw new TokenException("Session has expired.");
        }

        var user = await _commandDbContext.ApplicationUsers
            .FirstOrDefaultAsync(u => u.Id == session.UserId, cancellationToken);

        if (user is null)
        {
            throw new TokenException("User not found for this session.");
        }

        session.RevokedAtUtc = DateTime.UtcNow;

        var rawRefresh = GenerateRefreshToken();
        var saltBytes = HashingHelper.GenerateSalt();
        var hashB64 = HashingHelper.HashPassword(rawRefresh, saltBytes);
        var saltB64 = Convert.ToBase64String(saltBytes);

        var prefixLength = Math.Min(rawRefresh.Length, RefreshPrefixLength);
        var refreshPrefix = rawRefresh.Substring(0, prefixLength);

        var now = DateTimeOffset.UtcNow;
        var refreshExpires = now.AddMinutes(_settings.RefreshTokenMinutes);

        var newId = Guid.NewGuid();

        var newSession = new Session
        {
            SessionId = newId,
            RefreshTokenHash = hashB64,
            RefreshTokenSalt = saltB64,
            RefreshTokenPrefix = refreshPrefix,
            CreatedAtUtc = now.UtcDateTime,
            ExpiresAtUtc = refreshExpires.UtcDateTime,
            UserId = session.UserId,
            User = user
        };

        session.ReplacedBySessionId = newId;

        await _sessionRepository.AddAsync(newSession, cancellationToken);
        await _sessionRepository.SaveChangesAsync(cancellationToken);

        var accessToken = _jwtTokenProvider.CreateAccessToken(user, out var accessExpiresAt);

        return AuthMapper.ToRefreshResult(
            newSession,
            accessToken,
            rawRefresh,
            GenerateCsrfToken(),
            accessExpiresAt,
            refreshExpires);
    }

    public async Task<TokenInfoDto> GetTokenInfoAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var info = await _tokenRepository.GetTokenInfoAsync(sessionId, cancellationToken);
        if (info is null)
        {
            throw new TokenException("Session not found.");
        }

        return info;
    }

    private string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private string GenerateCsrfToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(16);
        return Convert.ToBase64String(bytes);
    }
}