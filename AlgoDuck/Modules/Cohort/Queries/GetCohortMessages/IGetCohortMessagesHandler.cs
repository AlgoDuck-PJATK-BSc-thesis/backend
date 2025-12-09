namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;

public interface IGetCohortMessagesHandler
{
    Task<GetCohortMessagesResultDto> HandleAsync(
        Guid userId,
        GetCohortMessagesRequestDto dto,
        CancellationToken cancellationToken);
}