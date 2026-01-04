namespace AlgoDuck.Modules.User.Commands.User.Profile.UpdateUsername;

public interface IUpdateUsernameHandler
{
    Task HandleAsync(Guid userId, UpdateUsernameDto dto, CancellationToken cancellationToken);
}