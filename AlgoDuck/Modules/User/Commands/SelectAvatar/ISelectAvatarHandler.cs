namespace AlgoDuck.Modules.User.Commands.SelectAvatar;

public interface ISelectAvatarHandler
{
    Task HandleAsync(Guid userId, SelectAvatarDto dto, CancellationToken cancellationToken);
}