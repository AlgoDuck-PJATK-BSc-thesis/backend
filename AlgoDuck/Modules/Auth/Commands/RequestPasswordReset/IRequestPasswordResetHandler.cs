namespace AlgoDuck.Modules.Auth.Commands.RequestPasswordReset;

public interface IRequestPasswordResetHandler
{
    Task HandleAsync(RequestPasswordResetDto dto, CancellationToken cancellationToken);
}