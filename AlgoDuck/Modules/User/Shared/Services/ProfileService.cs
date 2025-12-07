using AlgoDuck.Modules.User.Shared.Constants;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Utils;
using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class ProfileService : IProfileService
{
    private const string AvatarFolderPrefix = "Ducks/Outfits/";

    private readonly IUserRepository _userRepository;
    private readonly IS3AvatarUrlGenerator _avatarUrlGenerator;
    private readonly ApplicationQueryDbContext _queryDbContext;

    public ProfileService(
        IUserRepository userRepository,
        IS3AvatarUrlGenerator avatarUrlGenerator,
        ApplicationQueryDbContext queryDbContext)
    {
        _userRepository = userRepository;
        _avatarUrlGenerator = avatarUrlGenerator;
        _queryDbContext = queryDbContext;
    }

    public async Task<UserProfileDto> GetProfileAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

        var dto = ProfileMapper.ToUserProfileDto(user);

        var selectedItemId = await GetSelectedAvatarItemIdAsync(userId, cancellationToken);

        string avatarKey = string.Empty;
        if (selectedItemId.HasValue)
        {
            avatarKey = AvatarFolderPrefix + "duck-" + selectedItemId.Value.ToString("D") + ".png";
        }

        var s3AvatarUrl = _avatarUrlGenerator.GetAvatarUrl(avatarKey);

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
            S3AvatarUrl = s3AvatarUrl,
            TwoFactorEnabled = user.TwoFactorEnabled,
            EmailConfirmed = user.EmailConfirmed
        };
    }

    public async Task<ProfileUpdateResult> UpdateAvatarAsync(Guid userId, string avatarKey, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(avatarKey))
        {
            throw new ValidationException("Avatar key cannot be empty.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

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
        {
            throw new UserNotFoundException(userId);
        }

        if (string.IsNullOrWhiteSpace(newUsername))
        {
            throw new ValidationException("Username cannot be empty.");
        }

        if (newUsername.Length < ProfileConstants.MinUsernameLength ||
            newUsername.Length > ProfileConstants.MaxUsernameLength)
        {
            throw new ValidationException("Username length is invalid.");
        }

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
        {
            throw new ValidationException("Language cannot be empty.");
        }

        language = language.Trim().ToLowerInvariant();

        if (language != "en" && language != "pl")
        {
            throw new ValidationException("Language must be 'en' or 'pl'.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new UserNotFoundException(userId);
        }

        user.UserConfig ??= new Models.UserConfig { UserId = user.Id };
        user.UserConfig.Language = language;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new ProfileUpdateResult
        {
            Success = true,
            Message = "Language updated successfully."
        };
    }

    private async Task<Guid?> GetSelectedAvatarItemIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var selected = await _queryDbContext.Purchases
            .Where(p => p.UserId == userId && p.Selected)
            .Select(p => p.ItemId)
            .FirstOrDefaultAsync(cancellationToken);

        if (selected == Guid.Empty)
        {
            return null;
        }

        return selected;
    }
}