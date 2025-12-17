namespace AlgoDuck.Modules.Auth.Commands.EnableTwoFactor;

public interface IEnableTwoFactorHandler
{
    Task HandleAsync(Guid userId, CancellationToken cancellationToken);
}