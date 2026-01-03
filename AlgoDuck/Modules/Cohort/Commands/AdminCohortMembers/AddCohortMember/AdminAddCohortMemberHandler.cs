using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Commands.AdminCohortMembers.AddCohortMember;

public sealed class AdminAddCohortMemberHandler : IAdminAddCohortMemberHandler
{
    private readonly ApplicationCommandDbContext _db;
    private readonly IValidator<AdminAddCohortMemberDto> _validator;

    public AdminAddCohortMemberHandler(ApplicationCommandDbContext db, IValidator<AdminAddCohortMemberDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task HandleAsync(Guid cohortId, AdminAddCohortMemberDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.CohortId == cohortId, cancellationToken);
        if (cohort is null)
        {
            throw new CohortNotFoundException(cohortId);
        }

        var user = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == dto.UserId, cancellationToken);
        if (user is null)
        {
            throw new CohortValidationException("User not found.");
        }

        if (user.CohortId.HasValue)
        {
            throw new CohortValidationException("User already belongs to a cohort.");
        }

        var now = DateTime.UtcNow;

        user.CohortId = cohortId;
        user.CohortJoinedAt = now;

        if (cohort.EmptiedAt is not null || !cohort.IsActive)
        {
            cohort.EmptiedAt = null;
            cohort.IsActive = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }
}