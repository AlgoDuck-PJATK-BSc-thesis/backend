using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohort;

public sealed class JoinCohortHandler : IJoinCohortHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ICohortRepository _cohortRepository;

    public JoinCohortHandler(
        IUserRepository userRepository,
        ICohortRepository cohortRepository)
    {
        _userRepository = userRepository;
        _cohortRepository = cohortRepository;
    }

    public async Task<JoinCohortResultDto> HandleAsync(
        Guid userId,
        Guid cohortId,
        CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new CohortValidationException("User not found.");
        }

        if (user.CohortId.HasValue)
        {
            throw new CohortValidationException("Leave current cohort to join another.");
        }

        var cohort = await _cohortRepository.GetByIdAsync(cohortId, cancellationToken);
        if (cohort is null)
        {
            throw new CohortNotFoundException(cohortId);
        }

        if (!cohort.IsActive && cohort.EmptiedAt is null)
        {
            throw new CohortNotFoundException(cohortId);
        }

        user.CohortId = cohort.CohortId;
        user.CohortJoinedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new JoinCohortResultDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            CreatedByUserId = cohort.CreatedByUserId.GetValueOrDefault(),
            IsActive = cohort.IsActive
        };
    }
}