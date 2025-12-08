namespace AlgoDuck.Modules.User.Queries.GetSelectedAvatar;

public interface IGetSelectedAvatarHandler
{
    Task<SelectedAvatarDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}