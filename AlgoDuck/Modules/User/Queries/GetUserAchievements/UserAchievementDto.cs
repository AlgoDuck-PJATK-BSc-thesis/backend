
namespace AlgoDuck.Modules.User.Queries.GetUserAchievements;

public sealed class UserAchievementDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int CurrentValue { get; init; }
    public int TargetValue { get; init; }
    public bool IsCompleted { get; init; }
    public DateTime? CompletedAt { get; init; }
}