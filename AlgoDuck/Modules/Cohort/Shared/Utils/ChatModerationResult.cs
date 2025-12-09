namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public sealed class ChatModerationResult
{
    public bool IsAllowed { get; init; }
    public string? BlockReason { get; init; }
    public string? Category { get; init; }
    public double? Severity { get; init; }

    public static ChatModerationResult Allowed()
    {
        return new ChatModerationResult
        {
            IsAllowed = true
        };
    }

    public static ChatModerationResult Blocked(string reason, string? category = null, double? severity = null)
    {
        return new ChatModerationResult
        {
            IsAllowed = false,
            BlockReason = reason,
            Category = category,
            Severity = severity
        };
    }
}