using AlgoDuck.Modules.Cohort.Shared.Services;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Tests.Unit.Modules.Cohort.Shared.Services;

public sealed class ChatPresenceServiceTests
{
    private static ChatPresenceService CreateService(TimeSpan? idleTimeoutOverride = null)
    {
        var settings = new ChatPresenceSettings
        {
            IdleTimeout = idleTimeoutOverride ?? TimeSpan.FromMinutes(5)
        };

        var options = Options.Create(settings);
        return new ChatPresenceService(options);
    }

    [Fact]
    public async Task GetActiveUsersAsync_WhenNoPresence_ReturnsEmpty()
    {
        var service = CreateService();

        var cohortId = Guid.NewGuid();

        var result = await service.GetActiveUsersAsync(cohortId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UserConnectedAsync_ThenGetActiveUsersAsync_ReturnsUser()
    {
        var service = CreateService();

        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var connectionId = "conn-1";

        await service.UserConnectedAsync(
            cohortId,
            userId,
            connectionId,
            CancellationToken.None);

        var result = await service.GetActiveUsersAsync(cohortId, CancellationToken.None);

        result.Should().ContainSingle();
        result.Single().UserId.Should().Be(userId);
    }

    [Fact]
    public async Task UserDisconnectedAsync_WhenLastConnectionRemoved_UserIsNotActive()
    {
        var service = CreateService();

        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var connectionId = "conn-1";

        await service.UserConnectedAsync(
            cohortId,
            userId,
            connectionId,
            CancellationToken.None);

        await service.UserDisconnectedAsync(
            cohortId,
            userId,
            connectionId,
            CancellationToken.None);

        var result = await service.GetActiveUsersAsync(cohortId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Presence_IsTrackedPerCohort()
    {
        var service = CreateService();

        var cohortA = Guid.NewGuid();
        var cohortB = Guid.NewGuid();
        var userA = Guid.NewGuid();
        var userB = Guid.NewGuid();

        await service.UserConnectedAsync(
            cohortA,
            userA,
            "conn-a",
            CancellationToken.None);

        await service.UserConnectedAsync(
            cohortB,
            userB,
            "conn-b",
            CancellationToken.None);

        var activeA = await service.GetActiveUsersAsync(cohortA, CancellationToken.None);
        var activeB = await service.GetActiveUsersAsync(cohortB, CancellationToken.None);

        activeA.Should().ContainSingle();
        activeA.Single().UserId.Should().Be(userA);

        activeB.Should().ContainSingle();
        activeB.Single().UserId.Should().Be(userB);
    }

    [Fact]
    public async Task IdleTimeoutZero_QuicklyRemovesUsersWithNoConnections()
    {
        var service = CreateService(TimeSpan.Zero);

        var cohortId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var connectionId = "conn-1";

        await service.UserConnectedAsync(
            cohortId,
            userId,
            connectionId,
            CancellationToken.None);

        await service.UserDisconnectedAsync(
            cohortId,
            userId,
            connectionId,
            CancellationToken.None);

        var result = await service.GetActiveUsersAsync(cohortId, CancellationToken.None);

        result.Should().BeEmpty();
    }
}