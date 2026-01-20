namespace AlgoDuck.Modules.Cohort.Queries.User.Cohorts.GetCohortById;

public interface IGetCohortByIdHandler
{
    Task<GetCohortByIdResultDto> HandleAsync(Guid userId, GetCohortByIdRequestDto dto, CancellationToken cancellationToken);
}