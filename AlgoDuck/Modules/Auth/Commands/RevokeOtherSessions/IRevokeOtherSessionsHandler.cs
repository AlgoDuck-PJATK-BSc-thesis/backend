namespace AlgoDuck.Modules.Auth.Commands.RevokeOtherSessions;

public interface IRevokeOtherSessionsHandler
{
    Task HandleAsync(Guid userId, RevokeOtherSessionsDto dto, CancellationToken cancellationToken);
}