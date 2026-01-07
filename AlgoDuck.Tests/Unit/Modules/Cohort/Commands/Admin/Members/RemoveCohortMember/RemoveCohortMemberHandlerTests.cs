using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.Admin.Members.RemoveCohortMember;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.Admin.Members.RemoveCohortMember;

public sealed class RemoveCohortMemberHandlerTests
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
        await using var db = CreateDb();

        var handler = new RemoveCohortMemberHandler(db);

        var cohortId = Guid.NewGuid();
        var ex = await Assert.ThrowsAsync<CohortNotFoundException>(() =>
            handler.HandleAsync(cohortId, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal($"Cohort '{cohortId}' not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotFound_ThrowsCohortValidationException()
    {
        await using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C1",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new RemoveCohortMemberHandler(db);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(cohortId, Guid.NewGuid(), CancellationToken.None));

        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserNotMemberOfCohort_ThrowsCohortValidationException()
    {
        await using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C1",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        });

        var userId = Guid.NewGuid();
        db.ApplicationUsers.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "u1",
            CohortId = Guid.NewGuid(),
            CohortJoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new RemoveCohortMemberHandler(db);

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(cohortId, userId, CancellationToken.None));

        Assert.Equal("User is not a member of this cohort.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenRemovingLeavesNoMembers_DeactivatesAndSetsEmptiedAt()
    {
        await using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C1",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmptiedAt = null
        });

        var userId = Guid.NewGuid();
        db.ApplicationUsers.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "u1",
            CohortId = cohortId,
            CohortJoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new RemoveCohortMemberHandler(db);

        await handler.HandleAsync(cohortId, userId, CancellationToken.None);

        var updatedUser = await db.ApplicationUsers.AsTracking().FirstAsync(u => u.Id == userId);
        Assert.Null(updatedUser.CohortId);
        Assert.Null(updatedUser.CohortJoinedAt);

        var updatedCohort = await db.Cohorts.AsTracking().FirstAsync(c => c.CohortId == cohortId);
        Assert.False(updatedCohort.IsActive);
        Assert.NotNull(updatedCohort.EmptiedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenMembersRemain_DoesNotDeactivateCohort()
    {
        await using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C1",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            EmptiedAt = null
        });

        var removedUserId = Guid.NewGuid();
        var remainingUserId = Guid.NewGuid();

        db.ApplicationUsers.Add(new ApplicationUser
        {
            Id = removedUserId,
            UserName = "u1",
            CohortId = cohortId,
            CohortJoinedAt = DateTime.UtcNow
        });

        db.ApplicationUsers.Add(new ApplicationUser
        {
            Id = remainingUserId,
            UserName = "u2",
            CohortId = cohortId,
            CohortJoinedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new RemoveCohortMemberHandler(db);

        await handler.HandleAsync(cohortId, removedUserId, CancellationToken.None);

        var cohort = await db.Cohorts.AsTracking().FirstAsync(c => c.CohortId == cohortId);
        Assert.True(cohort.IsActive);
        Assert.Null(cohort.EmptiedAt);
    }
}
