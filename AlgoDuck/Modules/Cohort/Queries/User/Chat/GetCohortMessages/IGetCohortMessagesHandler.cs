namespace AlgoDuck.Modules.Cohort.Queries.User.Chat.GetCohortMessages;

public interface IGetCohortMessagesHandler
{
    Task<GetCohortMessagesResultDto> HandleAsync(
        Guid userId,
        GetCohortMessagesRequestDto dto,
        CancellationToken cancellationToken);
}