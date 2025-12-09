namespace AlgoDuck.Modules.Cohort.Queries.GetCohortById;

public interface IGetCohortByIdHandler
{
    Task<GetCohortByIdResultDto> HandleAsync(Guid userId, GetCohortByIdRequestDto dto, CancellationToken cancellationToken);
}