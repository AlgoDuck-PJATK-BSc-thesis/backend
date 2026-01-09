using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Services;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Interfaces;

public sealed class IChatPresenceServiceTests
{
    [Fact]
    public async Task UserConnectedAsync_ThenUserAppearsInActiveUsers()
    {
        var settings = Options.Create(new ChatPresenceSettings
        {
            IdleTimeout = TimeSpan.FromMinutes(5)
        });

        IChatPresenceService service = new ChatPresenceService(settings);

        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var connectionId = "conn-1";

        await service.UserConnectedAsync(cohortId, userId, connectionId, CancellationToken.None);

        var active = await service.GetActiveUsersAsync(cohortId, CancellationToken.None);

        active.Should().ContainSingle(u => u.UserId == userId);
    }

    [Fact]
    public async Task UserDisconnectedAsync_WhenNoConnections_RemainsAbsentFromActiveUsers()
    {
        var settings = Options.Create(new ChatPresenceSettings
        {
            IdleTimeout = TimeSpan.FromMinutes(5)
        });

        IChatPresenceService service = new ChatPresenceService(settings);

        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var connectionId = "conn-1";

        await service.UserConnectedAsync(cohortId, userId, connectionId, CancellationToken.None);
        await service.UserDisconnectedAsync(cohortId, userId, connectionId, CancellationToken.None);

        var active = await service.GetActiveUsersAsync(cohortId, CancellationToken.None);

        active.Should().BeEmpty();
    }
}