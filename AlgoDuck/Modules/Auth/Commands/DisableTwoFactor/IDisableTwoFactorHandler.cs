namespace AlgoDuck.Modules.Auth.Commands.DisableTwoFactor;

public interface IDisableTwoFactorHandler
{
    Task HandleAsync(Guid userId, DisableTwoFactorDto dto, CancellationToken cancellationToken);
}