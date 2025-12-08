using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetSelectedAvatar;

public sealed class GetSelectedAvatarHandler : IGetSelectedAvatarHandler
{
    private readonly IProfileService _profileService;

    public GetSelectedAvatarHandler(IProfileService profileService)
    {
        _profileService = profileService;
    }

    public async Task<SelectedAvatarDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("User identifier is invalid.");
        }

        var profile = await _profileService.GetProfileAsync(userId, cancellationToken);

        return new SelectedAvatarDto
        {
            UserId = profile.UserId,
            S3AvatarUrl = profile.S3AvatarUrl
        };
    }
}