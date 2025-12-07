using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.GetUserRankings;

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

        var users = await _dbContext.Users
            .OrderByDescending(u => u.Experience)
            .ThenBy(u => u.UserName)
            .Skip(skip)
            .Take(take)
            .Select((u, index) => new UserRankingDto
            {
                UserId = u.Id,
                Username = u.UserName ?? string.Empty,
                Experience = u.Experience,
                Rank = skip + index + 1
            })
            .ToListAsync(cancellationToken);

        return users;
    }
}