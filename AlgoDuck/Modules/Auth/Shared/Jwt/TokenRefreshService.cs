using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Shared.Jwt;

public sealed class TokenRefreshService
{
    private readonly JwtTokenProvider _jwtTokenProvider;
    private readonly TokenGenerator _tokenGenerator;
    private readonly JwtSettings _settings;

    public TokenRefreshService(
        JwtTokenProvider jwtTokenProvider,
        TokenGenerator tokenGenerator,
        IOptions<JwtSettings> options)
    {
        _jwtTokenProvider = jwtTokenProvider;
        _tokenGenerator = tokenGenerator;
        _settings = options.Value;
    }

    public Task<RefreshResult> CreateRefreshResultAsync(
        ApplicationUser user,
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var accessToken = _jwtTokenProvider.CreateAccessToken(user, out var accessExpiresAt);
        var refreshToken = _tokenGenerator.GenerateRefreshToken();
        var refreshExpiresAt = DateTimeOffset.UtcNow.AddMinutes(_settings.RefreshTokenMinutes);
        var csrfToken = _tokenGenerator.GenerateCsrfToken();

        var result = new RefreshResult
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            CsrfToken = csrfToken,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresAt,
            SessionId = sessionId,
            UserId = user.Id
        };

        return Task.FromResult(result);
    }
}