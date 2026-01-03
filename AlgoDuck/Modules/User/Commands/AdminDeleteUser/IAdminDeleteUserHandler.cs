namespace AlgoDuck.Modules.User.Commands.AdminDeleteUser;

public interface IAdminDeleteUserHandler
{
    Task HandleAsync(Guid userId, CancellationToken cancellationToken);
}