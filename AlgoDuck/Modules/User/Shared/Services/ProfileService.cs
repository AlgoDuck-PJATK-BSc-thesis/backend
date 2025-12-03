using AlgoDuck.Modules.User.Shared.Constants;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Utils;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class ProfileService : IProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly IS3AvatarUrlGenerator _avatarUrlGenerator;

    public ProfileService(
        IUserRepository userRepository,
        IS3AvatarUrlGenerator avatarUrlGenerator)
    {
        _userRepository = userRepository;
        _avatarUrlGenerator = avatarUrlGenerator;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(userId);

        var dto = ProfileMapper.ToUserProfileDto(user);
        var avatarUrl = _avatarUrlGenerator.GetAvatarUrl(dto.AvatarKey);

        return new UserProfileDto
        {
            UserId = dto.UserId,
            Username = dto.Username,
            Email = dto.Email,
            Coins = dto.Coins,
            Experience = dto.Experience,
            AmountSolved = dto.AmountSolved,
            CohortId = dto.CohortId,
            Language = dto.Language,
            AvatarKey = avatarUrl
        };
    }

    public async Task<ProfileUpdateResult> UpdateAvatarAsync(Guid userId, string avatarKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
            throw new ValidationException("Avatar key cannot be empty.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(userId);

        user.UserConfig ??= new AlgoDuck.Models.UserConfig { UserId = user.Id };
        user.UserConfig.AvatarKey = avatarKey;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new ProfileUpdateResult
        {
            Success = true,
            Message = "Avatar updated successfully."
        };
    }

    public async Task<ProfileUpdateResult> UpdateUsernameAsync(Guid userId, string newUsername, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(userId);

        if (string.IsNullOrWhiteSpace(newUsername))
            throw new ValidationException("Username cannot be empty.");

        if (newUsername.Length < ProfileConstants.MinUsernameLength ||
            newUsername.Length > ProfileConstants.MaxUsernameLength)
            throw new ValidationException("Username length is invalid.");

        user.UserName = newUsername;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new ProfileUpdateResult
        {
            Success = true,
            Message = "Username updated."
        };
    }

    public async Task<ProfileUpdateResult> UpdateLanguageAsync(Guid userId, string language, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(language))
            throw new ValidationException("Language cannot be empty.");

        language = language.Trim().ToLowerInvariant();

        if (language != "en" && language != "pl")
            throw new ValidationException("Language must be 'en' or 'pl'.");

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            throw new UserNotFoundException(userId);

        user.UserConfig ??= new AlgoDuck.Models.UserConfig { UserId = user.Id };
        user.UserConfig.Language = language;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new ProfileUpdateResult
        {
            Success = true,
            Message = "Language updated successfully."
        };
    }
}