using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.User.Chat.SendMessage;

public sealed class SendMessageHandler : ISendMessageHandler
{
    private readonly IValidator<SendMessageDto> _validator;
    private readonly ICohortRepository _cohortRepository;
    private readonly IChatMessageRepository _chatMessageRepository;
    private readonly IChatModerationService _chatModerationService;
    private readonly IProfileService _profileService;
    private readonly IChatMessageRateLimiter _rateLimiter;

    public SendMessageHandler(
        IValidator<SendMessageDto> validator,
        ICohortRepository cohortRepository,
        IChatMessageRepository chatMessageRepository,
        IChatModerationService chatModerationService,
        IProfileService profileService,
        IChatMessageRateLimiter rateLimiter)
    {
        _validator = validator;
        _cohortRepository = cohortRepository;
        _chatMessageRepository = chatMessageRepository;
        _chatModerationService = chatModerationService;
        _profileService = profileService;
        _rateLimiter = rateLimiter;
    }

    public async Task<SendMessageResultDto> HandleAsync(
        Guid userId,
        SendMessageDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid chat message payload.");
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, dto.CohortId, cancellationToken);
        if (!belongs)
        {
            throw new CohortValidationException("User does not belong to this cohort.");
        }

        await _rateLimiter.CheckAsync(userId, dto.CohortId, cancellationToken);

        if (dto.MediaType == ChatMediaType.Text)
        {
            var contentToModerate = dto.Content ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(contentToModerate))
            {
                var moderationResult = await _chatModerationService.CheckMessageAsync(
                    userId,
                    dto.CohortId,
                    contentToModerate,
                    cancellationToken);

                if (!moderationResult.IsAllowed)
                {
                    throw new ChatValidationException(
                        moderationResult.BlockReason ?? "This message violates our content rules.",
                        moderationResult.Category);
                }
            }
        }

        var mediaKey = string.IsNullOrWhiteSpace(dto.MediaKey) ? null : dto.MediaKey.Trim();
        var mediaContentType = string.IsNullOrWhiteSpace(dto.MediaContentType) ? null : dto.MediaContentType.Trim();

        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            CohortId = dto.CohortId,
            UserId = userId,
            Message1 = dto.MediaType == ChatMediaType.Text ? (dto.Content ?? string.Empty) : string.Empty,
            CreatedAt = DateTime.UtcNow,
            MediaType = (int)dto.MediaType,
            MediaKey = mediaKey,
            MediaContentType = mediaContentType
        };

        var saved = await _chatMessageRepository.AddAsync(message, cancellationToken);
        var profile = await _profileService.GetProfileAsync(saved.UserId, cancellationToken);

        var savedMediaType = (ChatMediaType)saved.MediaType;
        var mediaUrl = savedMediaType == ChatMediaType.Image && !string.IsNullOrWhiteSpace(saved.MediaKey)
            ? ChatMediaUrl.Build(saved.CohortId, saved.MediaKey!)
            : null;

        var result = ChatMessageMappings.ToSendMessageResultDto(
            saved,
            profile,
            savedMediaType,
            mediaUrl);

        result.ClientMessageId = dto.ClientMessageId;

        return result;
    }
}
