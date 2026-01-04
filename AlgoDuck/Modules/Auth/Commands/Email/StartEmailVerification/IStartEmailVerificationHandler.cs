namespace AlgoDuck.Modules.Auth.Commands.Email.StartEmailVerification;

public interface IStartEmailVerificationHandler
{
    Task HandleAsync(StartEmailVerificationDto dto, CancellationToken cancellationToken);
}