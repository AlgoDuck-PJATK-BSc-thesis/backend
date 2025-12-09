using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.JoinCohort;

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
            throw new CohortValidationException("User already belongs to a cohort.");
        }

        var cohort = await _cohortRepository.GetByIdAsync(cohortId, cancellationToken);
        if (cohort is null || !cohort.IsActive)
        {
            throw new CohortNotFoundException(cohortId);
        }

        user.CohortId = cohort.CohortId;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new JoinCohortResultDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            CreatedByUserId = cohort.CreatedByUserId,
            IsActive = cohort.IsActive
        };
    }
}