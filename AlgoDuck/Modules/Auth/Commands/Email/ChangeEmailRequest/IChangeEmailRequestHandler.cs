namespace AlgoDuck.Modules.Auth.Commands.Email.ChangeEmailRequest;

public interface IChangeEmailRequestHandler
{
    Task HandleAsync(Guid userId, ChangeEmailRequestDto dto, CancellationToken cancellationToken);
}