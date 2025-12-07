using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Interfaces;

namespace AlgoDuck.Modules.Auth.Shared.Services;

public sealed class SessionService
{
    private readonly ISessionRepository _sessionRepository;

    public SessionService(ISessionRepository sessionRepository)
    {
        _sessionRepository = sessionRepository;
    }

    public async Task<IReadOnlyList<Session>> GetUserSessionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new TokenException("User identifier is invalid.");
        }

        return await _sessionRepository.GetUserSessionsAsync(userId, cancellationToken);
    }

    public async Task RevokeSessionAsync(Guid userId, Guid sessionId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new TokenException("User identifier is invalid.");
        }

        if (sessionId == Guid.Empty)
        {
            throw new TokenException("Session identifier is invalid.");
        }

        var session = await _sessionRepository.GetByIdAsync(sessionId, cancellationToken);
        if (session is null)
        {
            throw new TokenException("Session not found.");
        }

        if (session.UserId != userId)
        {
            throw new PermissionException("You do not own this session.");
        }

        if (session.RevokedAtUtc.HasValue)
        {
            return;
        }

        session.RevokedAtUtc = DateTime.UtcNow;
        await _sessionRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeOtherSessionsAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new TokenException("User identifier is invalid.");
        }

        if (currentSessionId == Guid.Empty)
        {
            throw new TokenException("Session identifier is invalid.");
        }

        var sessions = await _sessionRepository.GetUserSessionsAsync(userId, cancellationToken);

        var now = DateTime.UtcNow;
        var changed = false;

        foreach (var session in sessions)
        {
            if (session.SessionId == currentSessionId)
            {
                continue;
            }

            if (session.RevokedAtUtc.HasValue)
            {
                continue;
            }

            session.RevokedAtUtc = now;
            changed = true;
        }

        if (changed)
        {
            await _sessionRepository.SaveChangesAsync(cancellationToken);
        }
    }
}