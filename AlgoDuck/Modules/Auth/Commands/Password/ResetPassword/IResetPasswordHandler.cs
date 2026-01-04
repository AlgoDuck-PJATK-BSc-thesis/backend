namespace AlgoDuck.Modules.Auth.Commands.Password.ResetPassword;

public interface IResetPasswordHandler
{
    Task HandleAsync(ResetPasswordDto dto, CancellationToken cancellationToken);
}