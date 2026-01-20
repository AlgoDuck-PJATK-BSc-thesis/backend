using AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;

public sealed class GetCohortsHandlerTests
{
    [Fact]
    public async Task HandleAsync_ReturnsPagedAndMapsItemsAndCursors()
    {
        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);

        var c1 = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = "C1",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedByUserId = Guid.NewGuid(),
            CreatedByUserLabel = "  Alice  "
        };

        var c2 = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = "C2",
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            CreatedByUserId = null,
            CreatedByUserLabel = "  Bob  "
        };

        repo.Setup(x => x.GetPagedAsync(2, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Models.Cohort> { c1, c2 }, 100));

        var handler = new AdminGetCohortsHandler(repo.Object);

        var result = await handler.HandleAsync(new AdminGetCohortsDto { Page = 2, PageSize = 3 }, CancellationToken.None);

        Assert.Equal(2, result.CurrPage);
        Assert.Equal(3, result.PageSize);
        Assert.Equal(100, result.TotalItems);
        Assert.Equal(1, result.PrevCursor);
        Assert.Equal(3, result.NextCursor);

        var items = result.Items.ToList();
        Assert.Equal(2, items.Count);

        var i1 = items[0];
        Assert.Equal(c1.CohortId, i1.CohortId);
        Assert.Equal("C1", i1.Name);
        Assert.True(i1.IsActive);
        Assert.Equal(c1.CreatedByUserId, i1.CreatedByUserId);
        Assert.Equal("Alice", i1.CreatedByDisplay);
        Assert.Equal(c1.CreatedAt, i1.CreatedAt);

        var i2 = items[1];
        Assert.Equal(c2.CohortId, i2.CohortId);
        Assert.Equal("C2", i2.Name);
        Assert.False(i2.IsActive);
        Assert.Null(i2.CreatedByUserId);
        Assert.Equal("Deleted user (Bob)", i2.CreatedByDisplay);
        Assert.Equal(c2.CreatedAt, i2.CreatedAt);

        repo.Verify(x => x.GetPagedAsync(2, 3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenNoMoreItems_SetsNextCursorNull()
    {
        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);

        repo.Setup(x => x.GetPagedAsync(2, 50, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Models.Cohort>(), 100));

        var handler = new AdminGetCohortsHandler(repo.Object);

        var result = await handler.HandleAsync(new AdminGetCohortsDto { Page = 2, PageSize = 50 }, CancellationToken.None);

        Assert.Equal(2, result.CurrPage);
        Assert.Equal(50, result.PageSize);
        Assert.Equal(100, result.TotalItems);
        Assert.Equal(1, result.PrevCursor);
        Assert.Null(result.NextCursor);
        Assert.Empty(result.Items);

        repo.Verify(x => x.GetPagedAsync(2, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCreatedByUserIdNotNullAndNoLabel_UsesGuidStringAsDisplay()
    {
        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);

        var createdBy = Guid.NewGuid();
        var c = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = "C",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedByUserId = createdBy,
            CreatedByUserLabel = "   "
        };

        repo.Setup(x => x.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Models.Cohort> { c }, 1));

        var handler = new AdminGetCohortsHandler(repo.Object);

        var result = await handler.HandleAsync(new AdminGetCohortsDto { Page = 1, PageSize = 20 }, CancellationToken.None);

        var item = result.Items.ToList()[0];
        Assert.Equal(createdBy.ToString(), item.CreatedByDisplay);

        repo.Verify(x => x.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenCreatedByUserIdNullAndNoLabel_UsesDeletedUser()
    {
        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);

        var c = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = "C",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            CreatedByUserId = null,
            CreatedByUserLabel = null
        };

        repo.Setup(x => x.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Models.Cohort> { c }, 1));

        var handler = new AdminGetCohortsHandler(repo.Object);

        var result = await handler.HandleAsync(new AdminGetCohortsDto { Page = 1, PageSize = 20 }, CancellationToken.None);

        var item = result.Items.ToList()[0];
        Assert.Equal("Deleted user", item.CreatedByDisplay);

        repo.Verify(x => x.GetPagedAsync(1, 20, It.IsAny<CancellationToken>()), Times.Once);
    }
}
