namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public sealed class ChatPresenceSettings
{
    public TimeSpan IdleTimeout { get; init; } = TimeSpan.FromMinutes(5);
}