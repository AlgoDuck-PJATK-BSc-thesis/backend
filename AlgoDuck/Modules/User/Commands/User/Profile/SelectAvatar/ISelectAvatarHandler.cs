namespace AlgoDuck.Modules.User.Commands.User.Profile.SelectAvatar;

public interface ISelectAvatarHandler
{
    Task HandleAsync(Guid userId, SelectAvatarDto dto, CancellationToken cancellationToken);
}