using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.LeaveCohort;

public sealed class LeaveCohortHandler : ILeaveCohortHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationCommandDbContext _commandDbContext;

    public LeaveCohortHandler(
        IUserRepository userRepository,
        ApplicationCommandDbContext commandDbContext)
    {
        _userRepository = userRepository;
        _commandDbContext = commandDbContext;
    }

    public async Task HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new CohortValidationException("User not found.");
        }

        if (!user.CohortId.HasValue)
        {
            throw new CohortValidationException("User does not belong to any cohort.");
        }

        var cohortId = user.CohortId.Value;

        var cohort = await _commandDbContext.Cohorts
            .FirstOrDefaultAsync(c => c.CohortId == cohortId, cancellationToken);

        if (cohort is null)
        {
            throw new CohortNotFoundException(cohortId);
        }

        user.CohortId = null;
        await _userRepository.UpdateAsync(user, cancellationToken);

        var remainingMembers = await _commandDbContext.ApplicationUsers
            .CountAsync(u => u.CohortId == cohortId, cancellationToken);

        if (remainingMembers == 0)
        {
            cohort.IsActive = false;
            await _commandDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}