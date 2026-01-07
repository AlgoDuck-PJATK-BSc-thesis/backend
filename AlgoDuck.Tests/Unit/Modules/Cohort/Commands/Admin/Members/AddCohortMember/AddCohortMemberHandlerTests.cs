using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Commands.Admin.Members.AddCohortMember;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.Admin.Members.AddCohortMember;

public sealed class AddCohortMemberHandlerTests
{
    static ApplicationCommandDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsFluentValidationException()
    {
        await using var db = CreateDb();

        var handler = new AddCohortMemberHandler(db, new AddCohortMemberValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new AddCohortMemberDto { UserId = Guid.Empty }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenCohortNotFound_ThrowsCohortNotFoundException()
    {
        await using var db = CreateDb();

        var handler = new AddCohortMemberHandler(db, new AddCohortMemberValidator());

        var cohortId = Guid.NewGuid();
        var dto = new AddCohortMemberDto { UserId = Guid.NewGuid() };

        var ex = await Assert.ThrowsAsync<CohortNotFoundException>(() =>
            handler.HandleAsync(cohortId, dto, CancellationToken.None));

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

        var handler = new AddCohortMemberHandler(db, new AddCohortMemberValidator());

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(cohortId, new AddCohortMemberDto { UserId = Guid.NewGuid() }, CancellationToken.None));

        Assert.Equal("User not found.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenUserAlreadyInCohort_ThrowsCohortValidationException()
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

        var handler = new AddCohortMemberHandler(db, new AddCohortMemberValidator());

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(cohortId, new AddCohortMemberDto { UserId = userId }, CancellationToken.None));

        Assert.Equal("User already belongs to a cohort.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortInactiveOrEmptied_ReactivatesAndClearsEmptiedAt_AndSetsUserCohortFields()
    {
        await using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "C1",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            IsActive = false,
            EmptiedAt = DateTime.UtcNow.AddDays(-1)
        });

        var userId = Guid.NewGuid();
        db.ApplicationUsers.Add(new ApplicationUser
        {
            Id = userId,
            UserName = "u1",
            CohortId = null,
            CohortJoinedAt = null
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new AddCohortMemberHandler(db, new AddCohortMemberValidator());

        await handler.HandleAsync(cohortId, new AddCohortMemberDto { UserId = userId }, CancellationToken.None);

        var updatedUser = await db.ApplicationUsers.AsTracking().FirstAsync(u => u.Id == userId);
        Assert.Equal(cohortId, updatedUser.CohortId);
        Assert.NotNull(updatedUser.CohortJoinedAt);

        var updatedCohort = await db.Cohorts.AsTracking().FirstAsync(c => c.CohortId == cohortId);
        Assert.True(updatedCohort.IsActive);
        Assert.Null(updatedCohort.EmptiedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortActiveAndNotEmptied_AddsMemberWithoutChangingCohortFlags()
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
            CohortId = null,
            CohortJoinedAt = null
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new AddCohortMemberHandler(db, new AddCohortMemberValidator());

        await handler.HandleAsync(cohortId, new AddCohortMemberDto { UserId = userId }, CancellationToken.None);

        var updatedCohort = await db.Cohorts.AsTracking().FirstAsync(c => c.CohortId == cohortId);
        Assert.True(updatedCohort.IsActive);
        Assert.Null(updatedCohort.EmptiedAt);
    }
}
