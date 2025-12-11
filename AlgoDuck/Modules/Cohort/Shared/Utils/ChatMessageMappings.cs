using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;
using AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;
using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public static class ChatMessageMappings
{
    public static SendMessageResultDto ToSendMessageResultDto(
        Message message,
        UserProfileDto profile,
        ChatMediaType mediaType,
        string? mediaUrl)
    {
        return new SendMessageResultDto
        {
            MessageId = message.MessageId,
            CohortId = message.CohortId,
            UserId = message.UserId,
            UserName = profile.Username,
            UserAvatarUrl = profile.S3AvatarUrl,
            Content = message.Message1,
            MediaType = mediaType,
            MediaUrl = mediaUrl,
            CreatedAt = message.CreatedAt
        };
    }

    public static GetCohortMessagesItemDto ToGetCohortMessagesItemDto(
        Message message,
        UserProfileDto profile,
        Guid currentUserId,
        ChatMediaType mediaType,
        string? mediaUrl)
    {
        return new GetCohortMessagesItemDto
        {
            MessageId = message.MessageId,
            CohortId = message.CohortId,
            UserId = message.UserId,
            UserName = profile.Username,
            UserAvatarUrl = profile.S3AvatarUrl,
            Content = message.Message1,
            MediaType = mediaType,
            MediaUrl = mediaUrl,
            CreatedAt = message.CreatedAt,
            IsMine = message.UserId == currentUserId
        };
    }
}