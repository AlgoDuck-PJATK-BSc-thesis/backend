namespace AlgoDuck.Modules.Cohort.Commands.User.Chat.SendMessage;

public interface ISendMessageHandler
{
    Task<SendMessageResultDto> HandleAsync(
        Guid userId,
        SendMessageDto dto,
        CancellationToken cancellationToken);
}