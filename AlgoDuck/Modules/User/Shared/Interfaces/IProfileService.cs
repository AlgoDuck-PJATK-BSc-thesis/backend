using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Shared.Interfaces;

public interface IProfileService
{
    Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<ProfileUpdateResult> UpdateAvatarAsync(Guid userId, string avatarKey, CancellationToken cancellationToken);
    Task<ProfileUpdateResult> UpdateUsernameAsync(Guid userId, string newUsername, CancellationToken cancellationToken);
    Task<ProfileUpdateResult> UpdateLanguageAsync(Guid userId, string language, CancellationToken cancellationToken);
}