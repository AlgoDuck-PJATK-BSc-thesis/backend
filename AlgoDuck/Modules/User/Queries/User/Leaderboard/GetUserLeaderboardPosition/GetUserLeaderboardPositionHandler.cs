using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserLeaderboardPosition;

public sealed class GetUserLeaderboardPositionHandler : IGetUserLeaderboardPositionHandler
{
    private const string AdminRoleName = "admin";

    private readonly ApplicationQueryDbContext _queryDbContext;

    public GetUserLeaderboardPositionHandler(ApplicationQueryDbContext queryDbContext)
    {
        _queryDbContext = queryDbContext;
    }

    public async Task<UserLeaderboardPositionDto> HandleAsync(Guid userId, CancellationToken cancellationToken)
    {
        var adminRoleId = await _queryDbContext.Set<IdentityRole<Guid>>()
            .Where(r => r.Name == AdminRoleName)
            .Select(r => r.Id)
            .SingleAsync(cancellationToken);

        var isAdmin = await _queryDbContext.Set<IdentityUserRole<Guid>>()
            .AnyAsync(ur => ur.RoleId == adminRoleId && ur.UserId == userId, cancellationToken);

        if (isAdmin)
        {
            throw new UserNotFoundException("User not found for leaderboard.");
        }

        var adminUserIds = _queryDbContext.Set<IdentityUserRole<Guid>>()
            .Where(ur => ur.RoleId == adminRoleId)
            .Select(ur => ur.UserId);

        var user = await _queryDbContext.ApplicationUsers
            .AsNoTracking()
            .Where(u => u.Id == userId)
            .Where(u => !adminUserIds.Contains(u.Id))
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

        var totalUsers = await _queryDbContext.ApplicationUsers
            .Where(u => !adminUserIds.Contains(u.Id))
            .CountAsync(cancellationToken);

        var betterCount = await _queryDbContext.ApplicationUsers
            .Where(u => !adminUserIds.Contains(u.Id))
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
