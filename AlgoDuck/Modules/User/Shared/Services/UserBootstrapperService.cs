using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class UserBootstrapperService : IUserBootstrapperService
{
    private readonly ApplicationCommandDbContext _context;
    private readonly IUserAchievementSyncService _achievementSyncService;
    private readonly IDefaultDuckService _defaultDuckService;

    public UserBootstrapperService(
        ApplicationCommandDbContext context,
        IUserAchievementSyncService achievementSyncService,
        IDefaultDuckService defaultDuckService)
    {
        _context = context;
        _achievementSyncService = achievementSyncService;
        _defaultDuckService = defaultDuckService;
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
        await _defaultDuckService.EnsureAlgoduckOwnedAndSelectedAsync(userId, cancellationToken);
    }
}