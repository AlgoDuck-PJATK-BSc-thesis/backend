using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortActiveMembers;

public sealed class GetCohortActiveMembersHandler : IGetCohortActiveMembersHandler
{
    private readonly IValidator<GetCohortActiveMembersRequestDto> _validator;
    private readonly ICohortRepository _cohortRepository;
    private readonly IChatPresenceService _chatPresenceService;
    private readonly IProfileService _profileService;

    public GetCohortActiveMembersHandler(
        IValidator<GetCohortActiveMembersRequestDto> validator,
        ICohortRepository cohortRepository,
        IChatPresenceService chatPresenceService,
        IProfileService profileService)
    {
        _validator = validator;
        _cohortRepository = cohortRepository;
        _chatPresenceService = chatPresenceService;
        _profileService = profileService;
    }

    public async Task<GetCohortActiveMembersResultDto> HandleAsync(
        Guid userId,
        GetCohortActiveMembersRequestDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid active members query.");
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, dto.CohortId, cancellationToken);
        if (!belongs)
        {
            throw new CohortValidationException("User does not belong to this cohort.");
        }

        var activeUsers = await _chatPresenceService.GetActiveUsersAsync(dto.CohortId, cancellationToken);
        if (activeUsers.Count == 0)
        {
            return new GetCohortActiveMembersResultDto
            {
                Items = Array.Empty<GetCohortActiveMembersItemDto>()
            };
        }

        var items = new List<GetCohortActiveMembersItemDto>(activeUsers.Count);

        foreach (var active in activeUsers)
        {
            var profile = await _profileService.GetProfileAsync(active.UserId, cancellationToken);

            items.Add(new GetCohortActiveMembersItemDto
            {
                UserId = active.UserId,
                UserName = profile.Username,
                UserAvatarUrl = profile.S3AvatarUrl,
                LastSeenAt = active.LastSeenAt,
                IsActive = true
            });
        }

        return new GetCohortActiveMembersResultDto
        {
            Items = items
        };
    }
}