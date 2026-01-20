using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetUserRankings;

public sealed class GetUserRankingsHandler : IGetUserRankingsHandler
{
    private readonly ApplicationQueryDbContext _dbContext;

    public GetUserRankingsHandler(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<UserRankingDto>> HandleAsync(GetUserRankingsQuery query, CancellationToken cancellationToken)
    {
        var skip = (query.Page - 1) * query.PageSize;
        var take = query.PageSize;

        var page = await _dbContext.Users
            .OrderByDescending(u => u.Experience)
            .ThenBy(u => u.UserName)
            .Skip(skip)
            .Take(take)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Experience
            })
            .ToListAsync(cancellationToken);

        var result = page
            .Select((u, index) => new UserRankingDto
            {
                UserId = u.Id,
                Username = u.UserName ?? string.Empty,
                Experience = u.Experience,
                Rank = skip + index + 1
            })
            .ToList()
            .AsReadOnly();

        return result;
    }
}