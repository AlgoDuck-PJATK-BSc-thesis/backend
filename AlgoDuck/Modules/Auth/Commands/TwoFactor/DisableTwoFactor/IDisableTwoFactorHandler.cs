namespace AlgoDuck.Modules.Auth.Commands.TwoFactor.DisableTwoFactor;

public interface IDisableTwoFactorHandler
{
    Task HandleAsync(Guid userId,  CancellationToken cancellationToken);
}