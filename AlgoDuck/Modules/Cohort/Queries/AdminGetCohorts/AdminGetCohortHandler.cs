using AlgoDuck.Modules.Cohort.Queries.AdminShared;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Cohort.Queries.AdminGetCohorts;

public sealed class AdminGetCohortsHandler : IAdminGetCohortsHandler
{
    private readonly ICohortRepository _cohortRepository;

    public AdminGetCohortsHandler(ICohortRepository cohortRepository)
    {
        _cohortRepository = cohortRepository;
    }

    public async Task<PageData<AdminCohortItemDto>> HandleAsync(AdminGetCohortsDto query, CancellationToken cancellationToken)
    {
        var (cohorts, total) = await _cohortRepository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);

        var items = cohorts
            .Select(c => new AdminCohortItemDto
            {
                CohortId = c.CohortId,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedByUserId = c.CreatedByUserId
            })
            .ToList();

        var prev = query.Page > 1 ? query.Page - 1 : (int?)null;
        var next = query.Page * query.PageSize < total ? query.Page + 1 : (int?)null;

        return new PageData<AdminCohortItemDto>
        {
            CurrPage = query.Page,
            PageSize = query.PageSize,
            TotalItems = total,
            PrevCursor = prev,
            NextCursor = next,
            Items = items
        };
    }
}