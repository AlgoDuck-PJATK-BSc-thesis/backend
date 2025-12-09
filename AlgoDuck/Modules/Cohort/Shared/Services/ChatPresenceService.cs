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
            _ => new UserPresenceState(now, new HashSet<string> { connectionId }),
            (_, existing) =>
            {
                existing.LastSeenAt = now;
                existing.ConnectionIds.Add(connectionId);
                return existing;
            });

        return Task.CompletedTask;
    }

    public Task UserDisconnectedAsync(Guid cohortId, Guid userId, string connectionId, CancellationToken cancellationToken)
    {
        if (!_cohortPresence.TryGetValue(cohortId, out var cohortMap))
        {
            return Task.CompletedTask;
        }

        if (!cohortMap.TryGetValue(userId, out var state))
        {
            return Task.CompletedTask;
        }

        state.ConnectionIds.Remove(connectionId);

        if (state.ConnectionIds.Count == 0)
        {
            state.LastSeenAt = DateTimeOffset.UtcNow;
        }

        return Task.CompletedTask;
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
            .Where(kvp => kvp.Value.LastSeenAt >= cutoff && kvp.Value.ConnectionIds.Count > 0)
            .Select(kvp => new CohortActiveUser
            {
                UserId = kvp.Key,
                LastSeenAt = kvp.Value.LastSeenAt
            })
            .ToList()
            .AsReadOnly();

        return Task.FromResult<IReadOnlyList<CohortActiveUser>>(result);
    }

    private sealed class UserPresenceState
    {
        public DateTimeOffset LastSeenAt;
        public HashSet<string> ConnectionIds;

        public UserPresenceState(DateTimeOffset lastSeenAt, HashSet<string> connectionIds)
        {
            LastSeenAt = lastSeenAt;
            ConnectionIds = connectionIds;
        }
    }
}