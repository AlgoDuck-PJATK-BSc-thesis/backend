namespace AlgoDuck.Modules.User.Queries.User.Profile.GetSelectedAvatar;

public interface IGetSelectedAvatarHandler
{
    Task<SelectedAvatarDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}