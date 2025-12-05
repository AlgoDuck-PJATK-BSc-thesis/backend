namespace AlgoDuck.Modules.Auth.Commands.ResetPassword;

public interface IResetPasswordHandler
{
    Task HandleAsync(ResetPasswordDto dto, CancellationToken cancellationToken);
}