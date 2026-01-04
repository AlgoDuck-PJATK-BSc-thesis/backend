using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetCohortLeaderboard;

public interface IGetCohortLeaderboardHandler
{
    Task<UserLeaderboardPageDto> HandleAsync(GetCohortLeaderboardRequestDto requestDto, CancellationToken cancellationToken);
}