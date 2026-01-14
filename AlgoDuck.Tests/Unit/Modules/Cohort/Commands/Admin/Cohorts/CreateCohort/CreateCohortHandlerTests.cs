using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;

public sealed class CreateCohortHandlerTests
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
        using var db = CreateDb();

        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);
        repo.Setup(x => x.JoinCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateCohortHandler(db, repo.Object, new CreateCohortValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new CreateCohortDto { Name = "" }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenNameWhitespace_ThrowsFluentValidationException()
    {
        using var db = CreateDb();

        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);
        repo.Setup(x => x.JoinCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateCohortHandler(db, repo.Object, new CreateCohortValidator());

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new CreateCohortDto { Name = "   " }, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_WhenCannotGenerateUniqueJoinCode_ThrowsCohortValidationException()
    {
        using var db = CreateDb();

        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);
        repo.Setup(x => x.JoinCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new CreateCohortHandler(db, repo.Object, new CreateCohortValidator());

        var ex = await Assert.ThrowsAsync<CohortValidationException>(() =>
            handler.HandleAsync(Guid.NewGuid(), new CreateCohortDto { Name = "Cohort A" }, CancellationToken.None));

        Assert.Equal("Failed to generate a unique join code. Please try again.", ex.Message);
    }

    [Fact]
    public async Task HandleAsync_WhenSuccess_CreatesCohortAndReturnsItem()
    {
        using var db = CreateDb();

        var repo = new Mock<ICohortRepository>(MockBehavior.Strict);
        repo.Setup(x => x.JoinCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var handler = new CreateCohortHandler(db, repo.Object, new CreateCohortValidator());

        var adminUserId = Guid.NewGuid();

        var result = await handler.HandleAsync(adminUserId, new CreateCohortDto { Name = "  Cohort A  " }, CancellationToken.None);

        var saved = await db.Cohorts.FirstOrDefaultAsync(c => c.CohortId == result.CohortId);
        Assert.NotNull(saved);

        Assert.Equal("Cohort A", saved.Name);
        Assert.True(saved.IsActive);
        Assert.Null(saved.EmptiedAt);
        Assert.Equal(adminUserId, saved.CreatedByUserId);

        Assert.Equal(saved.CohortId, result.CohortId);
        Assert.Equal("Cohort A", result.Name);
        Assert.True(result.IsActive);
        Assert.Equal(adminUserId, result.CreatedByUserId);

        repo.Verify(x => x.JoinCodeExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }
}
