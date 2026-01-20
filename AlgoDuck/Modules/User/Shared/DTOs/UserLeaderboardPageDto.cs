namespace AlgoDuck.Modules.User.Shared.DTOs;

public sealed class UserLeaderboardPageDto
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalUsers { get; init; }
    public IReadOnlyList<UserLeaderboardEntryDto> Entries { get; init; } = Array.Empty<UserLeaderboardEntryDto>();
}