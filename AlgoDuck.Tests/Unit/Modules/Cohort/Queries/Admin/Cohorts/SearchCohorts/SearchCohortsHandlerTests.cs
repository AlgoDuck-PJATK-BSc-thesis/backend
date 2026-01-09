using AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

public sealed class SearchCohortsHandlerTests
{
    [Fact]
    public async Task HandleAsync_WhenQueryIsGuid_UsesGetByIdForIdMatch_AndReturnsNamePage()
    {
        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);

        var cohortId = Guid.NewGuid();
        var idMatch = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C-ID",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedByUserId = null,
            CreatedByUserLabel = "  Bob  "
        };

        var searched1 = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = "C1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedByUserId = Guid.NewGuid(),
            CreatedByUserLabel = "  Alice  "
        };

        var searched2 = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = "C2",
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            CreatedByUserId = null,
            CreatedByUserLabel = null
        };

        repo.Setup(x => x.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(idMatch);

        repo.Setup(x => x.SearchByNamePagedAsync(cohortId.ToString(), 1, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Models.Cohort> { searched1, searched2 }, 10));

        var handler = new SearchCohortsHandler(repo.Object);

        var result = await handler.HandleAsync(new SearchCohortsDto
        {
            Query = $"  {cohortId}  ",
            Page = 1,
            PageSize = 2
        }, CancellationToken.None);

        Assert.NotNull(result.IdMatch);
        Assert.Equal(cohortId, result.IdMatch!.CohortId);
        Assert.Equal("C-ID", result.IdMatch.Name);
        Assert.False(string.IsNullOrWhiteSpace(result.IdMatch.CreatedByDisplay));
        Assert.Equal("Deleted user (Bob)", result.IdMatch.CreatedByDisplay);

        Assert.Equal(1, result.Name.CurrPage);
        Assert.Equal(2, result.Name.PageSize);
        Assert.Equal(10, result.Name.TotalItems);
        Assert.Null(result.Name.PrevCursor);
        Assert.Equal(2, result.Name.NextCursor);

        var items = result.Name.Items.ToList();
        Assert.Equal(2, items.Count);

        Assert.Equal(searched1.CohortId, items[0].CohortId);
        Assert.Equal("Alice", items[0].CreatedByDisplay);

        Assert.Equal(searched2.CohortId, items[1].CohortId);
        Assert.Equal("Deleted user", items[1].CreatedByDisplay);

        repo.Verify(x => x.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SearchByNamePagedAsync(cohortId.ToString(), 1, 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenQueryIsGuidButNotFound_IdMatchNull_StillReturnsNamePage()
    {
        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);

        var cohortId = Guid.NewGuid();

        repo.Setup(x => x.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Models.Cohort?)null);

        repo.Setup(x => x.SearchByNamePagedAsync(cohortId.ToString(), 2, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Models.Cohort>(), 7));

        var handler = new SearchCohortsHandler(repo.Object);

        var result = await handler.HandleAsync(new SearchCohortsDto
        {
            Query = cohortId.ToString(),
            Page = 2,
            PageSize = 5
        }, CancellationToken.None);

        Assert.Null(result.IdMatch);

        Assert.Equal(2, result.Name.CurrPage);
        Assert.Equal(5, result.Name.PageSize);
        Assert.Equal(7, result.Name.TotalItems);
        Assert.Equal(1, result.Name.PrevCursor);
        Assert.Null(result.Name.NextCursor);
        Assert.Empty(result.Name.Items);

        repo.Verify(x => x.GetByIdAsync(cohortId, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.SearchByNamePagedAsync(cohortId.ToString(), 2, 5, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenQueryIsNotGuid_DoesNotCallGetByIdAsync_UsesTrimmedQuery()
    {
        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);

        repo.Setup(x => x.SearchByNamePagedAsync("abc", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Models.Cohort>(), 0));

        var handler = new SearchCohortsHandler(repo.Object);

        var result = await handler.HandleAsync(new SearchCohortsDto
        {
            Query = "  abc  ",
            Page = 1,
            PageSize = 20
        }, CancellationToken.None);

        Assert.Null(result.IdMatch);
        Assert.Empty(result.Name.Items);

        repo.Verify(x => x.SearchByNamePagedAsync("abc", 1, 20, It.IsAny<CancellationToken>()), Times.Once);
        repo.Verify(x => x.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_BuildCreatedByDisplay_WhenCreatedByUserIdNotNullAndNoLabel_UsesGuidString()
    {
        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);

        var createdBy = Guid.NewGuid();
        var c = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = "C1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedByUserId = createdBy,
            CreatedByUserLabel = "   "
        };

        repo.Setup(x => x.SearchByNamePagedAsync("x", 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Models.Cohort> { c }, 1));

        var handler = new SearchCohortsHandler(repo.Object);

        var result = await handler.HandleAsync(new SearchCohortsDto { Query = "x", Page = 1, PageSize = 20 }, CancellationToken.None);

        var item = result.Name.Items.ToList()[0];
        Assert.Equal(createdBy.ToString(), item.CreatedByDisplay);

        repo.Verify(x => x.SearchByNamePagedAsync("x", 1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }
}
