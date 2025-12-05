using AlgoDuck.Modules.Auth.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Shared.Services;

namespace AlgoDuck.Modules.Auth.Queries.GetUserSessions;

public sealed class GetUserSessionsHandler : IGetUserSessionsHandler
{
    private readonly SessionService _sessionService;

    public GetUserSessionsHandler(SessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public async Task<IReadOnlyList<UserSessionDto>> HandleAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new TokenException("User identifier is invalid.");
        }

        if (currentSessionId == Guid.Empty)
        {
            throw new TokenException("Session identifier is invalid.");
        }

        var sessions = await _sessionService.GetUserSessionsAsync(userId, cancellationToken);

        return sessions
            .Select(s => new UserSessionDto
            {
                SessionId = s.SessionId,
                CreatedAt = DateTime.SpecifyKind(s.CreatedAtUtc, DateTimeKind.Utc),
                ExpiresAt = DateTime.SpecifyKind(s.ExpiresAtUtc, DateTimeKind.Utc),
                RevokedAt = s.RevokedAtUtc.HasValue
                    ? DateTime.SpecifyKind(s.RevokedAtUtc.Value, DateTimeKind.Utc)
                    : null,
                IsCurrent = s.SessionId == currentSessionId
            })
            .OrderByDescending(x => x.CreatedAt)
            .ToList();
    }
}