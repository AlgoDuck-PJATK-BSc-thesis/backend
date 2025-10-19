using AlgoDuck.Modules.Cohort.CohortManagement.Shared;

namespace AlgoDuck.Modules.Cohort.CohortManagement.Commands.CreateCohort;

public sealed class CreateCohortHandler
{
    private readonly ICohortRepository _repo;
    public CreateCohortHandler(ICohortRepository repo) => _repo = repo;

    public Task<Guid> HandleAsync(CreateCohortDto dto, Guid createdByUserId, CancellationToken ct)
        => _repo.CreateAsync(dto.Name, dto.ImageUrl, createdByUserId, ct);
}