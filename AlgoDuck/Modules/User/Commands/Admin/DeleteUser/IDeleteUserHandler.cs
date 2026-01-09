namespace AlgoDuck.Modules.User.Commands.Admin.DeleteUser;

public interface IDeleteUserHandler
{
    Task HandleAsync(Guid userId, CancellationToken cancellationToken);
}