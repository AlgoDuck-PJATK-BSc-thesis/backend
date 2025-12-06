namespace AlgoDuck.Modules.User.Commands.ChangePassword;

public interface IChangePasswordHandler
{
    Task HandleAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken);
}