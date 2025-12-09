namespace AlgoDuck.Modules.Cohort.Shared.Utils;

public sealed class ChatModerationSettings
{
    public bool Enabled { get; init; } = true;
    public string Model { get; init; } = "omni-moderation-latest";
    public int MaxInputLength { get; init; } = 512;
    public double SeverityThreshold { get; init; } = 0.5;
    public bool FailClosed { get; init; } = true;
}