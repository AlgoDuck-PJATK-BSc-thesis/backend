using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.Cohort.Queries.GetUserCohorts;

public sealed class GetUserCohortsHandler : IGetUserCohortsHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ICohortRepository _cohortRepository;

    public GetUserCohortsHandler(
        IUserRepository userRepository,
        ICohortRepository cohortRepository)
    {
        _userRepository = userRepository;
        _cohortRepository = cohortRepository;
    }

    public async Task<GetUserCohortsResultDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new CohortValidationException("User not found.");
        }

        if (!user.CohortId.HasValue)
        {
            return new GetUserCohortsResultDto
            {
                Items = Array.Empty<GetUserCohortsItemDto>()
            };
        }

        var cohort = await _cohortRepository.GetByIdAsync(user.CohortId.Value, cancellationToken);
        if (cohort is null || !cohort.IsActive)
        {
            return new GetUserCohortsResultDto
            {
                Items = Array.Empty<GetUserCohortsItemDto>()
            };
        }

        return new GetUserCohortsResultDto
        {
            Items = new[]
            {
                new GetUserCohortsItemDto
                {
                    CohortId = cohort.CohortId,
                    Name = cohort.Name,
                    IsActive = cohort.IsActive,
                    CreatedByUserId = cohort.CreatedByUserId
                }
            }
        };
    }
}