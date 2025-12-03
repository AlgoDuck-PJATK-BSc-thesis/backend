using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetUserProfile;

public sealed class GetUserProfileHandler : IGetUserProfileHandler
{
    private readonly IProfileService _profileService;

    public GetUserProfileHandler(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public Task<UserProfileDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        return _profileService.GetProfileAsync(userId, cancellationToken);
    }
}