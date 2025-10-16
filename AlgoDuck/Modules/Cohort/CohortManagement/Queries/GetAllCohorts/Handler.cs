using AlgoDuck.Modules.Cohort.CohortManagement.Shared;

namespace AlgoDuck.Modules.Cohort.CohortManagement.Queries.GetAllCohorts;

public sealed class GetAllCohortsHandler
{
    private readonly ICohortRepository _repo;
    public GetAllCohortsHandler(ICohortRepository repo) => _repo = repo;

    public Task<List<CohortDto>> HandleAsync(CancellationToken ct) => _repo.GetAllAsync(ct);
}