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
}