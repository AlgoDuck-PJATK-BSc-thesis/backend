using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.DeleteCohort;

public sealed class DeleteCohortHandler : IDeleteCohortHandler
{
    private readonly ApplicationCommandDbContext _db;

    public DeleteCohortHandler(ApplicationCommandDbContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(Guid cohortId, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.CohortId == cohortId, cancellationToken);
        if (cohort is null)
        {
            throw new CohortNotFoundException(cohortId);
        }

        var users = await _db.ApplicationUsers
            .Where(u => u.CohortId == cohortId)
            .ToListAsync(cancellationToken);

        foreach (var user in users)
        {
            user.CohortId = null;
            user.CohortJoinedAt = null;
        }

        cohort.IsActive = false;
        cohort.EmptiedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}