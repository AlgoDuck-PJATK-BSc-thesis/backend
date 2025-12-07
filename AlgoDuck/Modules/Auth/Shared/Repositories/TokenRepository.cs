using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Shared.Repositories;

public sealed class TokenRepository : ITokenRepository
{
    private readonly ISessionRepository _sessionRepository;

    public TokenRepository(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<TokenInfoDto?> GetTokenInfoAsync(Guid sessionId, CancellationToken cancellationToken)
    {
        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            return null;
        }

        return ToTokenInfoDto(session);
    }

    public async Task<IReadOnlyList<TokenInfoDto>> GetUserTokensAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await _sessionRepository.GetUserSessionsAsync(userId, cancellationToken);
        return sessions
            .Select(ToTokenInfoDto)
            .ToList();
    }

    private static TokenInfoDto ToTokenInfoDto(Session session)
    {
        var expiresAt = new DateTimeOffset(session.ExpiresAtUtc, TimeSpan.Zero);

        return new TokenInfoDto
        {
            Id = session.SessionId,
            UserId = session.UserId,
            SessionId = session.SessionId,
            ExpiresAt = expiresAt,
            IsRevoked = session.RevokedAtUtc.HasValue
        };
    }
}