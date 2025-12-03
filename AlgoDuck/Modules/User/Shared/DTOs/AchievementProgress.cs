namespace AlgoDuck.Modules.User.Shared.DTOs;

public sealed class AchievementProgress
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CurrentValue { get; init; }
    public int TargetValue { get; init; }
    public bool IsCompleted { get; init; }
}