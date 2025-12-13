using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Tests.Modules.Cohort.Shared.Utils;

public sealed class CohortMappingsTests
{
    [Fact]
    public void ToCohortSummaryDto_MapsAllFields()
    {
        var cohortId = Guid.NewGuid();
        var creatorId = Guid.NewGuid();

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "My Cohort",
            IsActive = true,
            CreatedByUserId = creatorId
        };

        var dto = CohortMappings.ToCohortSummaryDto(cohort);

        Assert.Equal(cohortId, dto.CohortId);
        Assert.Equal("My Cohort", dto.Name);
        Assert.True(dto.IsActive);
        Assert.Equal(creatorId, dto.CreatedByUserId);
    }
}