namespace AlgoDuck.Modules.Auth.Commands.VerifyEmail;

public interface IVerifyEmailHandler
{
    Task HandleAsync(VerifyEmailDto dto, CancellationToken cancellationToken);
}