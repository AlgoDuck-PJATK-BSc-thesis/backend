namespace AlgoDuck.Modules.User.Commands.UpdateUsername;

public interface IUpdateUsernameHandler
{
    Task HandleAsync(Guid userId, UpdateUsernameDto dto, CancellationToken cancellationToken);
}