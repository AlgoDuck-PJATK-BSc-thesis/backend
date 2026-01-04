namespace AlgoDuck.Modules.Auth.Commands.Session.RevokeOtherSessions;

public interface IRevokeOtherSessionsHandler
{
    Task HandleAsync(Guid userId, RevokeOtherSessionsDto dto, CancellationToken cancellationToken);
}