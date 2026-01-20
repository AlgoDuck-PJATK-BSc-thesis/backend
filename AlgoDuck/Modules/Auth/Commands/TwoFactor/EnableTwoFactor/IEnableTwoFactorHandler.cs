namespace AlgoDuck.Modules.Auth.Commands.TwoFactor.EnableTwoFactor;

public interface IEnableTwoFactorHandler
{
    Task HandleAsync(Guid userId, CancellationToken cancellationToken);
}