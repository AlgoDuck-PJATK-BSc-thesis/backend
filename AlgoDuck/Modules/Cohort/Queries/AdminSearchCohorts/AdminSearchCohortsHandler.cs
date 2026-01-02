using AlgoDuck.Modules.Cohort.Queries.AdminShared;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Cohort.Queries.AdminSearchCohorts;

public sealed class AdminSearchCohortsHandler : IAdminSearchCohortsHandler
{
    private readonly ICohortRepository _cohortRepository;

    public AdminSearchCohortsHandler(ICohortRepository cohortRepository)
    {
        _cohortRepository = cohortRepository;
    }

    public async Task<AdminSearchCohortsResultDto> HandleAsync(AdminSearchCohortsDto query, CancellationToken cancellationToken)
    {
        var q = (query.Query).Trim();

        AdminCohortItemDto? idMatch = null;

        if (Guid.TryParse(q, out var cohortId))
        {
            var c = await _cohortRepository.GetByIdAsync(cohortId, cancellationToken);

            if (c is not null)
            {
                idMatch = new AdminCohortItemDto
                {
                    CohortId = c.CohortId,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    CreatedByUserId = c.CreatedByUserId,
                    CreatedByDisplay = BuildCreatedByDisplay(c.CreatedByUserId, c.CreatedByUserLabel)
                };
            }
        }

        var (itemsRaw, total) = await _cohortRepository.SearchByNamePagedAsync(q, query.Page, query.PageSize, cancellationToken);

        var items = itemsRaw
            .Select(c => new AdminCohortItemDto
            {
                CohortId = c.CohortId,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByDisplay = BuildCreatedByDisplay(c.CreatedByUserId, c.CreatedByUserLabel)
            })
            .ToList();

        var prev = query.Page > 1 ? query.Page - 1 : (int?)null;
        var next = query.Page * query.PageSize < total ? query.Page + 1 : (int?)null;

        return new AdminSearchCohortsResultDto
        {
            IdMatch = idMatch,
            Name = new PageData<AdminCohortItemDto>
            {
                CurrPage = query.Page,
                PageSize = query.PageSize,
                TotalItems = total,
                PrevCursor = prev,
                NextCursor = next,
                Items = items
            }
        };
    }

    private static string BuildCreatedByDisplay(Guid? createdByUserId, string? createdByUserLabel)
    {
        var label = (createdByUserLabel ?? string.Empty).Trim();

        if (createdByUserId is null)
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                return $"Deleted user ({label})";
            }

            return "Deleted user";
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            return label;
        }

        return createdByUserId.Value.ToString();
    }
}
