using AlgoDuck.Modules.Cohort.Shared.Utils;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Utils;

public sealed class CohortActiveUserTests
{
    [Fact]
    public void ObjectInitializer_SetsProperties()
    {
        var userId = Guid.NewGuid();
        var lastSeen = DateTimeOffset.UtcNow;

        var activeUser = new CohortActiveUser
        {
            UserId = userId,
            LastSeenAt = lastSeen
        };

        Assert.Equal(userId, activeUser.UserId);
        Assert.Equal(lastSeen, activeUser.LastSeenAt);
    }
}