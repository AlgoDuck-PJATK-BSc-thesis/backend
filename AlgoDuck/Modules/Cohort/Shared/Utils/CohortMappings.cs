
namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public static class CohortMappings
{
    public static CohortSummaryDto ToCohortSummaryDto(Models.Cohort cohort)
    {
        return new CohortSummaryDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            IsActive = cohort.IsActive,
            CreatedByUserId = cohort.CreatedByUserId
        };
    }
}

public sealed class CohortSummaryDto
{
    public Guid CohortId { get; init; }
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid CreatedByUserId { get; init; }
}