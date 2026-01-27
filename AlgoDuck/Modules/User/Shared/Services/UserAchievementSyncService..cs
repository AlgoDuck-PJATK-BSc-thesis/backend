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
        await EnsureAchievementCatalogAsync(cancellationToken);

        var userMetrics = await _db.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Coins,
                u.Experience,
                u.AmountSolved,
                u.EmailConfirmed
            })
            .FirstOrDefaultAsync(cancellationToken);

        var coins = userMetrics?.Coins ?? 0;
        var xp = userMetrics?.Experience ?? 0;
        var solved = userMetrics?.AmountSolved ?? 0;
        var emailVerified = userMetrics?.EmailConfirmed ?? false;

        if (userMetrics is null && !createMissing)
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
                .Select(a => a.AchievementCode)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var def in Catalog)
            {
                if (existingCodes.Contains(def.Code))
                {
                    continue;
                }

                var current = ResolveMetricValue(def.Metric, coins, xp, solved, emailVerified);
                var completed = current >= def.TargetValue;

                var newAchievement = new UserAchievement
                {
                    UserId = userId,
                    AchievementCode = def.Code,
                    CurrentValue = current,
                    IsCompleted = completed,
                    CompletedAt = completed ? now : null,
                    CreatedAt = now
                };

                achievements.Add(newAchievement);
                _db.Set<UserAchievement>().Add(newAchievement);
            }
        }

        foreach (var a in achievements)
        {
            if (!defsByCode.TryGetValue(a.AchievementCode, out var def))
            {
                continue;
            }

            var metricValue = ResolveMetricValue(def.Metric, coins, xp, solved, emailVerified);

            if (metricValue > a.CurrentValue)
            {
                a.CurrentValue = metricValue;
            }

            var targetValue = def.TargetValue;
            if (!a.IsCompleted && a.CurrentValue >= targetValue)
            {
                a.IsCompleted = true;
                a.CompletedAt ??= now;
            }

            if (a.IsCompleted && a.CompletedAt is null)
            {
                a.CompletedAt = now;
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private async Task EnsureAchievementCatalogAsync(CancellationToken cancellationToken)
    {
        var existingCodes = await _db.Achievements
            .Select(a => a.Code)
            .ToListAsync(cancellationToken);

        var existingSet = existingCodes.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        foreach (var def in Catalog)
        {
            if (existingSet.Contains(def.Code))
            {
                var existing = await _db.Achievements.FindAsync(new object[] { def.Code }, cancellationToken);
                if (existing != null)
                {
                    existing.Name = def.Name;
                    existing.Description = def.Description;
                    existing.TargetValue = def.TargetValue;
                }
            }
            else
            {
                _db.Achievements.Add(new Achievement
                {
                    Code = def.Code,
                    Name = def.Name,
                    Description = def.Description,
                    TargetValue = def.TargetValue,
                    CreatedAt = now
                });
            }
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private static int ResolveMetricValue(string metric, int coins, int experience, int amountSolved, bool emailVerified)
    {
        if (string.Equals(metric, "coins", StringComparison.OrdinalIgnoreCase)) return coins;
        if (string.Equals(metric, "experience", StringComparison.OrdinalIgnoreCase)) return experience;
        if (string.Equals(metric, "amount_solved", StringComparison.OrdinalIgnoreCase)) return amountSolved;
    
        if (string.Equals(metric, "onboarding", StringComparison.OrdinalIgnoreCase))
        {
            return emailVerified ? 1 : 0;
        }

        return 0;
    }

}
