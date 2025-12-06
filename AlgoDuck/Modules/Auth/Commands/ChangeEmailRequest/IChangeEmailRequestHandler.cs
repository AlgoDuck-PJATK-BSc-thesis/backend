namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailRequest;

public interface IChangeEmailRequestHandler
{
    Task HandleAsync(Guid userId, ChangeEmailRequestDto dto, CancellationToken cancellationToken);
}