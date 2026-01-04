namespace AlgoDuck.Modules.Auth.Commands.Password.RequestPasswordReset;

public interface IRequestPasswordResetHandler
{
    Task HandleAsync(RequestPasswordResetDto dto, CancellationToken cancellationToken);
}