namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface ICohortRepository
{
    Task<Models.Cohort?> GetByIdAsync(Guid cohortId, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid cohortId, CancellationToken cancellationToken);
    Task<bool> UserBelongsToCohortAsync(Guid userId, Guid cohortId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Models.Cohort>> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
}