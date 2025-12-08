using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Queries.GetCohortLeaderboard;

public interface IGetCohortLeaderboardHandler
{
    Task<UserLeaderboardPageDto> HandleAsync(GetCohortLeaderboardRequestDto requestDto, CancellationToken cancellationToken);
}