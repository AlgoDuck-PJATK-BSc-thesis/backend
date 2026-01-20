namespace AlgoDuck.Modules.Auth.Commands.Session.RevokeSession;

public interface IRevokeSessionHandler
{
    Task HandleAsync(Guid userId, RevokeSessionDto dto, CancellationToken cancellationToken);
}