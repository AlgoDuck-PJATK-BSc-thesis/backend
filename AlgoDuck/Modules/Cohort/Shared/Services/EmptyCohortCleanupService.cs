using AlgoDuck.DAL;
using AlgoDuck.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Shared.Services;

public sealed class EmptyCohortCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmptyCohortCleanupService> _logger;

    public EmptyCohortCleanupService(IServiceScopeFactory scopeFactory, ILogger<EmptyCohortCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SweepOnce(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Empty cohort cleanup sweep failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Empty cohort cleanup delay failed.");
            }
        }
    }

    private async Task SweepOnce(CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationCommandDbContext>();

        var now = DateTime.UtcNow;
        var deleteBefore = now.AddMinutes(-5);

        var memberCounts = await db.Set<ApplicationUser>()
            .AsNoTracking()
            .Where(u => u.CohortId != null)
            .GroupBy(u => u.CohortId!.Value)
            .Select(g => new { CohortId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var counts = memberCounts.ToDictionary(x => x.CohortId, x => x.Count);

        var cohorts = await db.Set<Models.Cohort>().ToListAsync(ct);

        var toDelete = new List<Guid>();

        foreach (var c in cohorts)
        {
            var count = counts.TryGetValue(c.CohortId, out var n) ? n : 0;

            if (count == 0)
            {
                if (c.EmptiedAt == null)
                {
                    c.EmptiedAt = now;
                }

                if (c.IsActive)
                {
                    c.IsActive = false;
                }

                if (c.EmptiedAt <= deleteBefore)
                {
                    toDelete.Add(c.CohortId);
                }
            }
            else
            {
                if (c.EmptiedAt != null)
                {
                    c.EmptiedAt = null;
                }

                if (!c.IsActive)
                {
                    c.IsActive = true;
                }
            }
        }

        if (toDelete.Count > 0)
        {
            var stillHasMembers = await db.Set<ApplicationUser>()
                .AsNoTracking()
                .Where(u => u.CohortId != null && toDelete.Contains(u.CohortId.Value))
                .Select(u => u.CohortId!.Value)
                .Distinct()
                .ToListAsync(ct);

            var stillHasMembersSet = stillHasMembers.ToHashSet();
            var finalDelete = toDelete.Where(id => !stillHasMembersSet.Contains(id)).ToList();

            if (finalDelete.Count > 0)
            {
                var messages = await db.Set<Message>()
                    .Where(m => finalDelete.Contains(m.CohortId))
                    .ToListAsync(ct);

                db.RemoveRange(messages);

                var cohortsToDelete = await db.Set<Models.Cohort>()
                    .Where(c => finalDelete.Contains(c.CohortId))
                    .ToListAsync(ct);

                db.RemoveRange(cohortsToDelete);
            }
        }

        await db.SaveChangesAsync(ct);
    }
}
