using AlgoDuck.Modules.Cohort.Shared.DTOs;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

public sealed class SearchCohortsHandler : ISearchCohortsHandler
{
    private readonly ICohortRepository _cohortRepository;

    public SearchCohortsHandler(ICohortRepository cohortRepository)
    {
        _cohortRepository = cohortRepository;
    }

    public async Task<SearchCohortsResultDto> HandleAsync(SearchCohortsDto query, CancellationToken cancellationToken)
    {
        var q = (query.Query).Trim();

        CohortItemDto? idMatch = null;

        if (Guid.TryParse(q, out var cohortId))
        {
            var c = await _cohortRepository.GetByIdAsync(cohortId, cancellationToken);

            if (c is not null)
            {
                idMatch = new CohortItemDto
                {
                    CohortId = c.CohortId,
                    Name = c.Name,
                    IsActive = c.IsActive,
                    CreatedByUserId = c.CreatedByUserId,
                    CreatedByDisplay = BuildCreatedByDisplay(c.CreatedByUserId, c.CreatedByUserLabel),
                    CreatedAt = c.CreatedAt
                };
            }
        }

        var (itemsRaw, total) = await _cohortRepository.SearchByNamePagedAsync(q, query.Page, query.PageSize, cancellationToken);

        var items = itemsRaw
            .Select(c => new CohortItemDto
            {
                CohortId = c.CohortId,
                Name = c.Name,
                IsActive = c.IsActive,
                CreatedByUserId = c.CreatedByUserId,
                CreatedByDisplay = BuildCreatedByDisplay(c.CreatedByUserId, c.CreatedByUserLabel),
                CreatedAt = c.CreatedAt
            })
            .ToList();

        var prev = query.Page > 1 ? query.Page - 1 : (int?)null;
        var next = query.Page * query.PageSize < total ? query.Page + 1 : (int?)null;

        return new SearchCohortsResultDto
        {
            IdMatch = idMatch,
            Name = new PageData<CohortItemDto>
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
