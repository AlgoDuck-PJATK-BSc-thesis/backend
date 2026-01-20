using System.Collections.Concurrent;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Cohort.Shared.Services;

public sealed class ChatPresenceService : IChatPresenceService
{
    private readonly ChatPresenceSettings _settings;
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, UserPresenceState>> _cohortPresence;

    public ChatPresenceService(IOptions<ChatPresenceSettings> options)
    {
        _settings = options.Value;
        _cohortPresence = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, UserPresenceState>>();
    }

    public Task UserConnectedAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken)
    {
        var cohortMap = _cohortPresence.GetOrAdd(cohortId, _ => new ConcurrentDictionary<Guid, UserPresenceState>());
        var now = DateTimeOffset.UtcNow;

        cohortMap.AddOrUpdate(
            userId,
            _ => new UserPresenceState(now, now, new HashSet<string> { connectionId }),
            (_, existing) =>
            {
                var connections = new HashSet<string>(existing.ConnectionIds) { connectionId };
                var lastActivity = existing.LastActivityAt;
                var lastSeen = now;
                return new UserPresenceState(lastActivity, lastSeen, connections);
            });

        return Task.CompletedTask;
    }

    public Task UserDisconnectedAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken)
    {
        if (!_cohortPresence.TryGetValue(cohortId, out var cohortMap))
        {
            return Task.CompletedTask;
        }

        while (true)
        {
            if (!cohortMap.TryGetValue(userId, out var existing))
            {
                return Task.CompletedTask;
            }

            var connections = new HashSet<string>(existing.ConnectionIds);
            connections.Remove(connectionId);

            var now = DateTimeOffset.UtcNow;
            var lastSeen = connections.Count == 0 ? now : existing.LastSeenAt;

            var updated = new UserPresenceState(existing.LastActivityAt, lastSeen, connections);

            if (cohortMap.TryUpdate(userId, updated, existing))
            {
                return Task.CompletedTask;
            }
        }
    }

    public Task<IReadOnlyList<CohortActiveUser>> GetActiveUsersAsync(Guid cohortId, CancellationToken cancellationToken)
    {
        if (!_cohortPresence.TryGetValue(cohortId, out var cohortMap))
        {
            return Task.FromResult<IReadOnlyList<CohortActiveUser>>(Array.Empty<CohortActiveUser>());
        }

        var now = DateTimeOffset.UtcNow;
        var cutoff = now - _settings.IdleTimeout;

        var result = cohortMap
            .Where(kvp => kvp.Value.ConnectionIds.Count > 0 && kvp.Value.LastActivityAt >= cutoff)
            .Select(kvp => new CohortActiveUser
            {
                UserId = kvp.Key,
                LastSeenAt = kvp.Value.LastActivityAt
            })
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<CohortActiveUser>>(result);
    }

    public Task<ChatPresenceSnapshot> ReportActivityAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken)
    {
        var cohortMap = _cohortPresence.GetOrAdd(cohortId, _ => new ConcurrentDictionary<Guid, UserPresenceState>());
        var now = DateTimeOffset.UtcNow;

        cohortMap.AddOrUpdate(
            userId,
            _ => new UserPresenceState(now, now, new HashSet<string> { connectionId }),
            (_, existing) =>
            {
                var connections = new HashSet<string>(existing.ConnectionIds);
                connections.Add(connectionId);
                return new UserPresenceState(now, now, connections);
            });

        return GetSnapshotAsync(cohortId, userId, cancellationToken);
    }

    public Task<ChatPresenceSnapshot> GetSnapshotAsync(Guid cohortId, Guid userId, CancellationToken cancellationToken)
    {
        if (!_cohortPresence.TryGetValue(cohortId, out var cohortMap) || !cohortMap.TryGetValue(userId, out var state))
        {
            var now = DateTimeOffset.UtcNow;
            return Task.FromResult(new ChatPresenceSnapshot
            {
                UserId = userId,
                Status = ChatPresenceStatus.Offline,
                LastActivityAt = now,
                LastSeenAt = now,
                ConnectionCount = 0,
                IsActiveLegacy = false
            });
        }

        return Task.FromResult(ToSnapshot(userId, state));
    }

    public Task<IReadOnlyList<ChatPresenceSnapshot>> GetSnapshotsForCohortAsync(Guid cohortId, CancellationToken cancellationToken)
    {
        if (!_cohortPresence.TryGetValue(cohortId, out var cohortMap))
        {
            return Task.FromResult<IReadOnlyList<ChatPresenceSnapshot>>(Array.Empty<ChatPresenceSnapshot>());
        }

        var list = cohortMap.Select(kvp => ToSnapshot(kvp.Key, kvp.Value)).ToList().AsReadOnly();
        return Task.FromResult<IReadOnlyList<ChatPresenceSnapshot>>(list);
    }

    private ChatPresenceSnapshot ToSnapshot(Guid userId, UserPresenceState state)
    {
        var now = DateTimeOffset.UtcNow;
        var connectionCount = state.ConnectionIds.Count;

        var status = connectionCount == 0
            ? ChatPresenceStatus.Offline
            : (now - state.LastActivityAt <= _settings.ActiveWindow ? ChatPresenceStatus.Active : ChatPresenceStatus.Away);

        var isActiveLegacy = connectionCount > 0 && now - state.LastActivityAt <= _settings.IdleTimeout;

        return new ChatPresenceSnapshot
        {
            UserId = userId,
            Status = status,
            LastActivityAt = state.LastActivityAt,
            LastSeenAt = state.LastSeenAt,
            ConnectionCount = connectionCount,
            IsActiveLegacy = isActiveLegacy
        };
    }

    private sealed class UserPresenceState
    {
        public DateTimeOffset LastActivityAt { get; }
        public DateTimeOffset LastSeenAt { get; }
        public HashSet<string> ConnectionIds { get; }

        public UserPresenceState(DateTimeOffset lastActivityAt, DateTimeOffset lastSeenAt, HashSet<string> connectionIds)
        {
            LastActivityAt = lastActivityAt;
            LastSeenAt = lastSeenAt;
            ConnectionIds = connectionIds;
        }
    }
}
