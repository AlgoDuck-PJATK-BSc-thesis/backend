namespace AlgoDuck.Modules.User.Commands.DeleteUser;

public interface IDeleteUserHandler
{
    Task HandleAsync(Guid userId, CancellationToken cancellationToken);
}