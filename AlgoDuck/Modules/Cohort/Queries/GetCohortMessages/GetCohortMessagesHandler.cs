using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;

public sealed class GetCohortMessagesHandler : IGetCohortMessagesHandler
{
    private readonly IValidator<GetCohortMessagesRequestDto> _validator;
    private readonly ICohortRepository _cohortRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IProfileService _profileService;

    public GetCohortMessagesHandler(
        IValidator<GetCohortMessagesRequestDto> validator,
        ICohortRepository cohortRepository,
        IChatMessageRepository chatMessageRepository,
        IProfileService profileService)
    {
        _validator = validator;
        _cohortRepository = cohortRepository;
        _chatMessageRepository = chatMessageRepository;
        _profileService = profileService;
    }

    public async Task<GetCohortMessagesResultDto> HandleAsync(
        Guid userId,
        GetCohortMessagesRequestDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid messages query.");
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, dto.CohortId, cancellationToken);
        if (!belongs)
        {
            throw new CohortValidationException("User does not belong to this cohort.");
        }

        var pageSize = dto.PageSize ?? ChatConstants.DefaultPageSize;

        var messages = await _chatMessageRepository.GetPagedForCohortAsync(
            dto.CohortId,
            dto.BeforeCreatedAt,
            pageSize + 1,
            cancellationToken);

        var hasMore = messages.Count > pageSize;
        if (hasMore)
        {
            messages = messages.Take(pageSize).ToList();
        }

        var items = new List<GetCohortMessagesItemDto>(messages.Count);

        foreach (var message in messages)
        {
            var profile = await _profileService.GetProfileAsync(message.UserId, cancellationToken);

            items.Add(ChatMessageMappings.ToGetCohortMessagesItemDto(
                message,
                profile,
                userId,
                ChatMediaType.Text,
                null));
        }

        DateTime? nextCursor = items.Count > 0
            ? items.Min(i => i.CreatedAt)
            : null;

        return new GetCohortMessagesResultDto
        {
            Items = items,
            NextCursor = nextCursor,
            HasMore = hasMore
        };
    }
}