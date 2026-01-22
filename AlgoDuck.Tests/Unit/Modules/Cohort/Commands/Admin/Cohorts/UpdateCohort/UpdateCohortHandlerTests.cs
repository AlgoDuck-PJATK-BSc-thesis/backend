using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;

public sealed class UpdateCohortHandlerTests
{
    static ApplicationCommandDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationCommandDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationCommandDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_WhenDtoInvalid_ThrowsCohortValidationException()
    {
        using var db = CreateDb();

        var handler = new UpdateCohortHandler(db, new UpdateCohortValidator());

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new UpdateCohortDto { Name = "" }, CancellationToken.None));

        Assert.Equal("Cohort's name is required.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenCohortNotFound_ThrowsCohortNotFoundException()
    {
        using var db = CreateDb();

        var handler = new UpdateCohortHandler(db, new UpdateCohortValidator());

        await Assert.ThrowsAsync<CohortNotFoundException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new UpdateCohortDto { Name = "Cohort A" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenNameWhitespace_ThrowsCohortValidationException()
    {
        using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Old",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = Guid.NewGuid(),
            IsActive = true
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateCohortHandler(db, new UpdateCohortValidator());

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(cohortId, new UpdateCohortDto { Name = "   " }, CancellationToken.None));

        Assert.Equal("Cohort's name is required.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenNameTooShort_ThrowsCohortValidationException()
    {
        using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Old",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = Guid.NewGuid(),
            IsActive = true
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateCohortHandler(db, new UpdateCohortValidator());

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(cohortId, new UpdateCohortDto { Name = "ab" }, CancellationToken.None));

        Assert.Equal("The length of Cohort's name must be at least 3 characters. You entered 2 characters.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccess_UpdatesNameAndReturnsItem()
    {
        using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        var createdBy = Guid.NewGuid();

        db.Cohorts.Add(new Models.Cohort
        {
            CohortId = cohortId,
            Name = "Old",
            JoinCode = "ABCDEFGHJK",
            CreatedAt = DateTime.UtcNow,
            CreatedByUserId = createdBy,
            IsActive = true,
            EmptiedAt = null
        });

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new UpdateCohortHandler(db, new UpdateCohortValidator());

        var result = await handler.HandleAsync(
            cohortId,
            new UpdateCohortDto { Name = "  New Name  " },
            CancellationToken.None);

        var saved = await db.Cohorts.FirstAsync(c => c.CohortId == cohortId);
        Assert.Equal("New Name", saved.Name);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal("New Name", result.Name);
        Assert.Equal(createdBy, result.CreatedByUserId);
    }
}
