using AlgoDuck.Modules.Cohort.CohortManagement.Queries.GetAllCohorts;

namespace AlgoDuck.Modules.Cohort.CohortManagement.Shared;

public interface ICohortRepository
{
    Task<List<CohortDto>> GetAllAsync(CancellationToken ct);
    Task<Guid> CreateAsync(string name, string imageUrl, Guid createdByUserId, CancellationToken ct);
}