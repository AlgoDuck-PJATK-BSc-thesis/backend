using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Queries.GetLeaderboardGlobal;

public interface IGetLeaderboardGlobalHandler
{
    Task<UserLeaderboardPageDto> HandleAsync(GetLeaderboardGlobalRequestDto requestDto, CancellationToken cancellationToken);
}