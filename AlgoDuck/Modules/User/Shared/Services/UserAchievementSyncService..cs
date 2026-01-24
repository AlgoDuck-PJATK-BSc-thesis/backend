using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class UserAchievementSyncService : IUserAchievementSyncService
{
    private readonly ApplicationCommandDbContext _db;

    private sealed record AchievementDefinition(
        string Code,
        string Name,
        string Description,
        string Metric,
        int TargetValue
    );

    private static readonly IReadOnlyList<AchievementDefinition> Catalog = new List<AchievementDefinition>
    {
        new("SOLVE_001", "First Steps", "Solve your first problem.", "amount_solved", 1),
        new("SOLVE_005", "Warmed Up", "Solve 5 problems.", "amount_solved", 5),
        new("SOLVE_010", "On a Roll", "Solve 10 problems.", "amount_solved", 10),
        new("SOLVE_025", "Quarter Century", "Solve 25 problems.", "amount_solved", 25),
        new("SOLVE_050", "Problem Grinder", "Solve 50 problems.", "amount_solved", 50),

        new("EXP_0100", "Getting Started", "Reach 100 XP.", "experience", 100),
        new("EXP_0500", "Leveling Up", "Reach 500 XP.", "experience", 500),
        new("EXP_1000", "Seasoned", "Reach 1,000 XP.", "experience", 1000),
        new("EXP_2500", "Battle Tested", "Reach 2,500 XP.", "experience", 2500),
        new("EXP_5000", "Veteran", "Reach 5,000 XP.", "experience", 5000),

        new("COIN_0100", "Pocket Change", "Hold 100 coins at once.", "coins", 100),
        new("COIN_1000", "Coin Collector", "Hold 1,000 coins at once.", "coins", 1000),
        new("COIN_5000", "Piggy Bank", "Hold 5,000 coins at once.", "coins", 5000),
        new("COIN_10000", "Treasure Chest", "Hold 10,000 coins at once.", "coins", 10000),
        new("COIN_25000", "Golden Hoard", "Hold 25,000 coins at once.", "coins", 25000)
    };

    public UserAchievementSyncService(ApplicationCommandDbContext db)
    {
        _db = db;
    }

    public Task EnsureInitializedAsync(Guid userId, CancellationToken cancellationToken)
    {
        return EnsureInitializedAndSyncInternalAsync(userId, createMissing: true, cancellationToken);
    }

    public Task SyncAsync(Guid userId, CancellationToken cancellationToken)
    {
        return EnsureInitializedAndSyncInternalAsync(userId, createMissing: false, cancellationToken);
    }

    private async Task EnsureInitializedAndSyncInternalAsync(Guid userId, bool createMissing, CancellationToken cancellationToken)
    {
        var userMetrics = await _db.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Coins,
                u.Experience,
                u.AmountSolved
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (userMetrics is null)
        {
            return;
        }

        var defsByCode = Catalog.ToDictionary(d => d.Code, StringComparer.OrdinalIgnoreCase);

        var achievements = await _db.Set<UserAchievement>()
            .Where(a => a.UserId == userId)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;

        if (createMissing)
        {
            var existingCodes = achievements
                .Select(a => a.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var def in Catalog)
            {
                if (existingCodes.Contains(def.Code))
                {
                    continue;
                }

                var current = ResolveMetricValue(def.Metric, userMetrics.Coins, userMetrics.Experience, userMetrics.AmountSolved);
                var completed = current >= def.TargetValue;

                achievements.Add(new UserAchievement
                {
                    UserId = userId,
                    Code = def.Code,
                    Name = def.Name,
                    Description = def.Description,
                    TargetValue = def.TargetValue,
                    CurrentValue = current,
                    IsCompleted = completed,
                    CompletedAt = completed ? now : null
                });
            }
        }

        foreach (var a in achievements)
        {
            if (!defsByCode.TryGetValue(a.Code, out var def))
            {
                continue;
            }

            if (!string.Equals(a.Name, def.Name, StringComparison.Ordinal))
            {
                a.Name = def.Name;
            }

            if (!string.Equals(a.Description, def.Description, StringComparison.Ordinal))
            {
                a.Description = def.Description;
            }

            if (a.TargetValue != def.TargetValue)
            {
                a.TargetValue = def.TargetValue;
            }

            var metricValue = ResolveMetricValue(def.Metric, userMetrics.Coins, userMetrics.Experience, userMetrics.AmountSolved);

            if (metricValue > a.CurrentValue)
            {
                a.CurrentValue = metricValue;
            }

            if (!a.IsCompleted && a.CurrentValue >= a.TargetValue)
            {
                a.IsCompleted = true;
                a.CompletedAt ??= now;
            }

            if (a.IsCompleted && a.CompletedAt is null)
            {
                a.CompletedAt = now;
            }
        }

        var hasNew = _db.ChangeTracker.Entries<UserAchievement>().Any(e => e.State == EntityState.Added);
        var hasUpdates = _db.ChangeTracker.Entries<UserAchievement>().Any(e => e.State == EntityState.Modified);

        if (hasNew || hasUpdates)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }
    }

    private static int ResolveMetricValue(string metric, int coins, int experience, int amountSolved)
    {
        if (string.Equals(metric, "coins", StringComparison.OrdinalIgnoreCase))
        {
            return coins;
        }

        if (string.Equals(metric, "experience", StringComparison.OrdinalIgnoreCase))
        {
            return experience;
        }

        if (string.Equals(metric, "amount_solved", StringComparison.OrdinalIgnoreCase))
        {
            return amountSolved;
        }

        return 0;
    }
}
