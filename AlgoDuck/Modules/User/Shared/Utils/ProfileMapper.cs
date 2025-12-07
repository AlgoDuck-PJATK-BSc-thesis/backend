using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Shared.Utils;

public static class ProfileMapper
{
    public static UserProfileDto ToUserProfileDto(ApplicationUser user)
    {
        var config = user.UserConfig;

        return new UserProfileDto
        {
            UserId = user.Id,
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Coins = user.Coins,
            Experience = user.Experience,
            AmountSolved = user.AmountSolved,
            CohortId = user.CohortId,
            Language = config?.Language ?? string.Empty,
            S3AvatarUrl = string.Empty
        };
    }
}