using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.DeleteCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.Admin.Cohorts.DeleteCohort;

public sealed class DeleteCohortHandlerTests
{
    static ApplicationCommandDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortNotFound_ThrowsCohortNotFoundException()
    {
        using var db = CreateDb();
        var handler = new DeleteCohortHandler(db);

        await Assert.ThrowsAsync<CohortNotFoundException>(() =>
            handler.HandleAsync(Guid.NewGuid(), CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenSuccess_UnassignsUsersAndDeactivatesCohort()
    {
        using var db = CreateDb();

        var cohortId = Guid.NewGuid();

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C1",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var u1 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u1", CohortId = cohortId, CohortJoinedAt = DateTime.UtcNow };
        var u2 = new ApplicationUser { Id = Guid.NewGuid(), UserName = "u2", CohortId = cohortId, CohortJoinedAt = DateTime.UtcNow };

        db.Cohorts.Add(cohort);
        db.ApplicationUsers.Add(u1);
        db.ApplicationUsers.Add(u2);

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new DeleteCohortHandler(db);

        await handler.HandleAsync(cohortId, CancellationToken.None);

        var updatedUsers = await db.ApplicationUsers.AsTracking().Where(u => u.Id == u1.Id || u.Id == u2.Id).ToListAsync();
        Assert.Equal(2, updatedUsers.Count);
        Assert.All(updatedUsers, u => Assert.Null(u.CohortId));
        Assert.All(updatedUsers, u => Assert.Null(u.CohortJoinedAt));

        var updatedCohort = await db.Cohorts.AsTracking().FirstAsync(c => c.CohortId == cohortId);
        Assert.False(updatedCohort.IsActive);
        Assert.NotNull(updatedCohort.EmptiedAt);
    }
}
