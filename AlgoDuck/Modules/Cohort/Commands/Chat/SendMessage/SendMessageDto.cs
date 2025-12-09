using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Modules.Cohort.Commands.Chat.SendMessage;

public sealed class SendMessageDto
{
    public Guid CohortId { get; init; }
    public string? Content { get; init; }
    public ChatMediaType MediaType { get; init; } = ChatMediaType.Text;
    public string? MediaKey { get; init; }
    public string? MediaContentType { get; init; }
}