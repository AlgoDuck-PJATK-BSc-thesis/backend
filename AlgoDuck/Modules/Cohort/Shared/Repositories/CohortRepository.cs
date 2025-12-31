using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Shared.Repositories;

public sealed class CohortRepository : ICohortRepository
{
    private readonly ApplicationQueryDbContext _queryDb;
    private readonly ApplicationCommandDbContext _commandDb;

    public CohortRepository(ApplicationQueryDbContext queryDb, ApplicationCommandDbContext commandDb)
    {
        _queryDb = queryDb;
        _commandDb = commandDb;
    }

    public async Task<Models.Cohort?> GetByIdAsync(Guid cohortId, CancellationToken cancellationToken)
    {
        return await _queryDb.Cohorts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CohortId == cohortId, cancellationToken);
    }

    public async Task<Models.Cohort?> GetByJoinCodeAsync(string joinCode, CancellationToken cancellationToken)
    {
        joinCode = (joinCode).Trim();

        return await _queryDb.Cohorts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.JoinCode == joinCode, cancellationToken);
    }

    public async Task<bool> JoinCodeExistsAsync(string joinCode, CancellationToken cancellationToken)
    {
        joinCode = (joinCode).Trim();

        return await _queryDb.Cohorts
            .AsNoTracking()
            .AnyAsync(c => c.JoinCode == joinCode, cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid cohortId, CancellationToken cancellationToken)
    {
        return await _queryDb.Cohorts
            .AsNoTracking()
            .AnyAsync(c => c.CohortId == cohortId, cancellationToken);
    }

    public async Task<bool> UserBelongsToCohortAsync(Guid userId, Guid cohortId, CancellationToken cancellationToken)
    {
        return await _queryDb.ApplicationUsers
            .AsNoTracking()
            .AnyAsync(u => u.Id == userId && u.CohortId == cohortId, cancellationToken);
    }

    public async Task<IReadOnlyList<Models.Cohort>> GetForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _queryDb.Cohorts
            .AsNoTracking()
            .Where(c => c.ApplicationUsers.Any(u => u.Id == userId))
            .ToListAsync(cancellationToken);
    }

    public Task AddAsync(Models.Cohort cohort, CancellationToken cancellationToken)
    {
        _commandDb.Cohorts.Add(cohort);
        return Task.CompletedTask;
    }

    public async Task<(IReadOnlyList<Models.Cohort> Items, int TotalCount)> GetPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var q = _queryDb.Cohorts.AsNoTracking();

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }

    public async Task<(IReadOnlyList<Models.Cohort> Items, int TotalCount)> SearchByNamePagedAsync(
        string query,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var normalized = (query).Trim();
        if (string.IsNullOrWhiteSpace(normalized))
            return (Array.Empty<Models.Cohort>(), 0);

        var cohortsQuery = _queryDb.Cohorts.AsNoTracking();

        var isInMemory = _queryDb.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";
        var lower = normalized.ToLowerInvariant();
        var like = "%" + lower + "%";

        IQueryable<Models.Cohort> q;

        if (isInMemory)
            q = cohortsQuery.Where(c => c.Name.ToLower().Contains(lower));
        else
            q = cohortsQuery.Where(c => EF.Functions.ILike(c.Name, like));

        var total = await q.CountAsync(cancellationToken);

        var items = await q
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, total);
    }
}
