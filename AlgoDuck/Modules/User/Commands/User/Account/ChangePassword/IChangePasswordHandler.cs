namespace AlgoDuck.Modules.User.Commands.User.Account.ChangePassword;

public interface IChangePasswordHandler
{
    Task HandleAsync(Guid userId, ChangePasswordDto dto, CancellationToken cancellationToken);
}