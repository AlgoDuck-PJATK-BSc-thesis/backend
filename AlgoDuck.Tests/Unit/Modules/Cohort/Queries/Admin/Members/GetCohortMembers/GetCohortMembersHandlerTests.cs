using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;

public sealed class GetCohortMembersHandlerTests
{
    static ApplicationQueryDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationQueryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationQueryDbContext(options);
    }

    [Fact]
    public async Task HandleAsync_ReturnsMembersForCohort_OrderedByUserName_AndTrimsFields()
    {
        await using var db = CreateDb();

        var cohortId = Guid.NewGuid();
        var otherCohortId = Guid.NewGuid();

        var u1 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "  bob  ",
            Email = "  bob@b.com  ",
            CohortId = cohortId,
            CohortJoinedAt = DateTime.UtcNow.AddDays(-2)
        };

        var u2 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "alice",
            Email = null,
            CohortId = cohortId,
            CohortJoinedAt = null
        };

        var u3 = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = "zzz",
            Email = "zzz@b.com",
            CohortId = otherCohortId,
            CohortJoinedAt = DateTime.UtcNow
        };

        db.ApplicationUsers.Add(u1);
        db.ApplicationUsers.Add(u2);
        db.ApplicationUsers.Add(u3);

        await db.SaveChangesAsync(CancellationToken.None);

        var handler = new GetCohortMembersHandler(db);

        var result = await handler.HandleAsync(new GetCohortMembersRequestDto { CohortId = cohortId }, CancellationToken.None);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal(2, result.TotalMembers);

        var members = result.Members.ToList();
        Assert.Equal(2, members.Count);

        Assert.Equal(u1.Id, members[0].UserId);
        Assert.Equal("bob", members[0].UserName);
        Assert.Equal("bob@b.com", members[0].Email);
        Assert.Equal(u1.CohortJoinedAt, members[0].JoinedAt);

        Assert.Equal(u2.Id, members[1].UserId);
        Assert.Equal("alice", members[1].UserName);
        Assert.Equal(string.Empty, members[1].Email);
        Assert.Null(members[1].JoinedAt);
    }

    [Fact]
    public async Task HandleAsync_WhenNoMembers_ReturnsEmptyList()
    {
        await using var db = CreateDb();

        var handler = new GetCohortMembersHandler(db);

        var cohortId = Guid.NewGuid();
        var result = await handler.HandleAsync(new GetCohortMembersRequestDto { CohortId = cohortId }, CancellationToken.None);

        Assert.Equal(cohortId, result.CohortId);
        Assert.Equal(0, result.TotalMembers);
        Assert.Empty(result.Members);
    }
}
