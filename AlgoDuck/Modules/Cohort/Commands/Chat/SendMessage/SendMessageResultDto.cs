using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;

public sealed class SendMessageResultDto
{
    public Guid MessageId { get; init; }
    public Guid CohortId { get; init; }
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string? UserAvatarUrl { get; init; }
    public string? Content { get; init; }
    public ChatMediaType MediaType { get; init; } = ChatMediaType.Text;
    public string? MediaUrl { get; init; }
    public DateTime CreatedAt { get; init; }
}