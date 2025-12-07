using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Queries.GetUserProfile;

public interface IGetUserProfileHandler
{
    Task<UserProfileDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}