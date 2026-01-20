namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public enum ChatPresenceStatus
{
    Active,
    Away,
    Offline
}

public sealed class ChatPresenceSnapshot
{
    public Guid UserId { get; init; }
    public ChatPresenceStatus Status { get; init; }
    public DateTimeOffset LastActivityAt { get; init; }
    public DateTimeOffset LastSeenAt { get; init; }
    public int ConnectionCount { get; init; }
    public bool IsActiveLegacy { get; init; }
}