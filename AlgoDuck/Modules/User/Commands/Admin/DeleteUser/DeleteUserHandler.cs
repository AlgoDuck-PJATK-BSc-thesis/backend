using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.DeleteUser;

public sealed class DeleteUserHandler : IDeleteUserHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationCommandDbContext _db;

    public DeleteUserHandler(UserManager<ApplicationUser> userManager, ApplicationCommandDbContext db)
    {
        _userManager = userManager;
        _db = db;
    }

    public async Task HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new ValidationException("User identifier is invalid.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException("User not found.");
        }

        var cohortId = user.CohortId;

        var label =
            !string.IsNullOrWhiteSpace(user.UserName) ? user.UserName.Trim() :
            !string.IsNullOrWhiteSpace(user.Email) ? user.Email.Trim() :
            userId.ToString();

        var createdCohorts = await _db.Cohorts
            .Where(c => c.CreatedByUserId == userId)
            .ToListAsync(cancellationToken);

        foreach (var c in createdCohorts)
        {
            c.CreatedByUserId = null;
            if (string.IsNullOrWhiteSpace(c.CreatedByUserLabel))
            {
                c.CreatedByUserLabel = label;
            }
        }

        if (createdCohorts.Count > 0)
        {
            await _db.SaveChangesAsync(cancellationToken);
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            var errorMessage = deleteResult.Errors.FirstOrDefault()?.Description ?? "Failed to delete user.";
            throw new ValidationException(errorMessage);
        }

        if (cohortId.HasValue)
        {
            var cohort = await _db.Cohorts
                .FirstOrDefaultAsync(c => c.CohortId == cohortId.Value, cancellationToken);

            if (cohort is not null)
            {
                var remainingMembers = await _db.ApplicationUsers
                    .AsNoTracking()
                    .CountAsync(u => u.CohortId == cohortId.Value, cancellationToken);

                if (remainingMembers == 0)
                {
                    cohort.IsActive = false;
                    cohort.EmptiedAt ??= DateTime.UtcNow;
                }
                else
                {
                    if (cohort.EmptiedAt is not null || !cohort.IsActive)
                    {
                        cohort.EmptiedAt = null;
                        cohort.IsActive = true;
                    }
                }

                await _db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}
