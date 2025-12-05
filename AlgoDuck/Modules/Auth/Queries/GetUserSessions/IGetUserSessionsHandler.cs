namespace AlgoDuck.Modules.Auth.Queries.GetUserSessions;

public interface IGetUserSessionsHandler
{
    Task<IReadOnlyList<UserSessionDto>> HandleAsync(Guid userId, Guid currentSessionId, CancellationToken cancellationToken);
}