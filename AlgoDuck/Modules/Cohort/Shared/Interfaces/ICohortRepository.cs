namespace AlgoDuck.Modules.Cohort.Shared.Interfaces;

public interface ICohortRepository
{
    Task<Models.Cohort?> GetByIdAsync(Guid cohortId, CancellationToken cancellationToken);
    Task<Models.Cohort?> GetByJoinCodeAsync(string joinCode, CancellationToken cancellationToken);
    Task<bool> JoinCodeExistsAsync(string joinCode, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(Guid cohortId, CancellationToken cancellationToken);
    Task<bool> UserBelongsToCohortAsync(Guid userId, Guid cohortId, CancellationToken cancellationToken);
    Task<IReadOnlyList<Models.Cohort>> GetForUserAsync(Guid userId, CancellationToken cancellationToken);
    Task AddAsync(Models.Cohort cohort, CancellationToken cancellationToken);

    Task<(IReadOnlyList<Models.Cohort> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<(IReadOnlyList<Models.Cohort> Items, int TotalCount)> SearchByNamePagedAsync(string query, int page, int pageSize, CancellationToken cancellationToken);
}