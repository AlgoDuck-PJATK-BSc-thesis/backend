namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMessages;

public sealed class GetCohortMessagesRequestDto
{
    public Guid CohortId { get; init; }
    public DateTime? BeforeCreatedAt { get; init; }
    public int? PageSize { get; init; }
}