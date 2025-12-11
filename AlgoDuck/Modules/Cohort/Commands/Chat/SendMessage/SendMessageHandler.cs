using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;

public sealed class SendMessageHandler : ISendMessageHandler
{
    private readonly IValidator<SendMessageDto> _validator;
    private readonly ICohortRepository _cohortRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IChatModerationService _chatModerationService;
    private readonly IProfileService _profileService;

    public SendMessageHandler(
        IValidator<SendMessageDto> validator,
        ICohortRepository cohortRepository,
        IChatMessageRepository chatMessageRepository,
        IChatModerationService chatModerationService,
        IProfileService profileService)
    {
        _validator = validator;
        _cohortRepository = cohortRepository;
        _chatMessageRepository = chatMessageRepository;
        _chatModerationService = chatModerationService;
        _profileService = profileService;
    }

    public async Task<SendMessageResultDto> HandleAsync(
        Guid userId,
        SendMessageDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, CancellationToken.None);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid chat message payload.");
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, dto.CohortId, CancellationToken.None);
        if (!belongs)
        {
            throw new CohortValidationException("User does not belong to this cohort.");
        }

        var contentToModerate = dto.MediaType == ChatMediaType.Text ? dto.Content ?? string.Empty : string.Empty;

        if (!string.IsNullOrWhiteSpace(contentToModerate))
        {
            var moderationResult = await _chatModerationService.CheckMessageAsync(
                userId,
                dto.CohortId,
                contentToModerate,
                CancellationToken.None);

            if (!moderationResult.IsAllowed)
            {
                throw new ChatValidationException(
                    moderationResult.BlockReason ?? "This message violates our content rules.",
                    moderationResult.Category);
            }
        }

        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            CohortId = dto.CohortId,
            UserId = userId,
            Message1 = dto.Content ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        var saved = await _chatMessageRepository.AddAsync(message, CancellationToken.None);

        var profile = await _profileService.GetProfileAsync(saved.UserId, CancellationToken.None);

        return ChatMessageMappings.ToSendMessageResultDto(
            saved,
            profile,
            ChatMediaType.Text,
            null);
    }
}