using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Commands.AdminCohortMembers.RemoveCohortMember;

public sealed class AdminRemoveCohortMemberHandler : IAdminRemoveCohortMemberHandler
{
    private readonly ApplicationCommandDbContext _db;

    public AdminRemoveCohortMemberHandler(ApplicationCommandDbContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(Guid cohortId, Guid userId, CancellationToken cancellationToken)
    {
        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.CohortId == cohortId, cancellationToken);
        if (cohort is null)
        {
            throw new CohortNotFoundException(cohortId);
        }

        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user is null)
        {
            throw new CohortValidationException("User not found.");
        }

        if (user.CohortId != cohortId)
        {
            throw new CohortValidationException("User is not a member of this cohort.");
        }

        user.CohortId = null;
        user.CohortJoinedAt = null;

        await _db.SaveChangesAsync(cancellationToken);

        var remainingMembers = await _db.ApplicationUsers.CountAsync(u => u.CohortId == cohortId, cancellationToken);
        if (remainingMembers == 0)
        {
            cohort.IsActive = false;
            cohort.EmptiedAt ??= DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}