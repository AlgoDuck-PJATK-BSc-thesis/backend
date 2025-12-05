namespace AlgoDuck.Modules.Auth.Commands.StartEmailVerification;

public interface IStartEmailVerificationHandler
{
    Task HandleAsync(StartEmailVerificationDto dto, CancellationToken cancellationToken);
}