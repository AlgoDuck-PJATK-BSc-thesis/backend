namespace AlgoDuck.Modules.Auth.Commands.RevokeSession;

public interface IRevokeSessionHandler
{
    Task HandleAsync(Guid userId, RevokeSessionDto dto, CancellationToken cancellationToken);
}