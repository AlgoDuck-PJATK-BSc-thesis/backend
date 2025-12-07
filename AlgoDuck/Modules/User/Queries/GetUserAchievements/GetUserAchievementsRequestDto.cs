namespace AlgoDuck.Modules.User.Queries.GetUserAchievements;

public sealed class GetUserAchievementsRequestDto
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public bool? Completed { get; init; }
    public string? CodeFilter { get; init; }
}