namespace AlgoDuck.Modules.Auth.Commands.Email.VerifyEmail;

public interface IVerifyEmailHandler
{
    Task HandleAsync(VerifyEmailDto dto, CancellationToken cancellationToken);
}