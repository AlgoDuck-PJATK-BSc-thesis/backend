using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.GetUserLeaderboardPosition;

public sealed class GetUserLeaderboardPositionHandler : IGetUserLeaderboardPositionHandler
{
    private readonly ApplicationQueryDbContext _queryDbContext;

    public GetUserLeaderboardPositionHandler(ApplicationQueryDbContext queryDbContext)
    {
        _queryDbContext = queryDbContext;
    }

    public async Task<UserLeaderboardPositionDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _queryDbContext.ApplicationUsers
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => new
            {
                u.Id,
                u.Experience,
                u.AmountSolved
            })
            .SingleOrDefaultAsync(cancellationToken);

        if (user is null)
        {
            throw new UserNotFoundException("User not found for leaderboard.");
        }

        var totalUsers = await _queryDbContext.ApplicationUsers.CountAsync(cancellationToken);

        var betterCount = await _queryDbContext.ApplicationUsers
            .CountAsync(
                u =>
                    u.Experience > user.Experience ||
                    (u.Experience == user.Experience && u.AmountSolved > user.AmountSolved),
                cancellationToken);

        var rank = betterCount + 1;

        double percentile = 0;
        if (totalUsers > 0)
        {
            percentile = 100.0 * (totalUsers - rank) / totalUsers;
        }

        return new UserLeaderboardPositionDto
        {
            UserId = user.Id,
            Rank = rank,
            TotalUsers = totalUsers,
            Experience = user.Experience,
            AmountSolved = user.AmountSolved,
            Percentile = percentile
        };
    }
}