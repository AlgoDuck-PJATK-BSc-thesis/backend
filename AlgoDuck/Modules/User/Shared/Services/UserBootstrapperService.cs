using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class UserBootstrapperService : IUserBootstrapperService
{
    private readonly ApplicationCommandDbContext _context;
    private readonly IUserAchievementSyncService _achievementSyncService;

    public UserBootstrapperService(
        ApplicationCommandDbContext context,
        IUserAchievementSyncService achievementSyncService)
    {
        _context = context;
        _achievementSyncService = achievementSyncService;
    }

    public async Task EnsureUserInitializedAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var hasConfig = await _context.UserConfigs
            .AsNoTracking()
            .AnyAsync(x => x.UserId == userId, cancellationToken);

        if (!hasConfig)
        {
            _context.UserConfigs.Add(new UserConfig
            {
                UserId = userId,
                EditorFontSize = 11,
                EmailNotificationsEnabled = false,
                IsDarkMode = true,
                IsHighContrast = false
            });

            await _context.SaveChangesAsync(cancellationToken);
        }

        await _achievementSyncService.EnsureInitializedAsync(userId, cancellationToken);
    }
}