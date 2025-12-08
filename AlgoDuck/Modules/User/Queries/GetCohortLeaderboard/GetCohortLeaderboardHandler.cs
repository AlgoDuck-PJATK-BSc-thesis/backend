using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.GetCohortLeaderboard;

public sealed class GetCohortLeaderboardHandler : IGetCohortLeaderboardHandler
{
    private readonly ApplicationQueryDbContext _queryDbContext;

    public GetCohortLeaderboardHandler(ApplicationQueryDbContext queryDbContext)
    {
        _queryDbContext = queryDbContext;
    }

    public async Task<UserLeaderboardPageDto> HandleAsync(GetCohortLeaderboardRequestDto requestDto, CancellationToken cancellationToken)
    {
        if (requestDto.CohortId == Guid.Empty)
        {
            throw new Shared.Exceptions.ValidationException("Cohort identifier is invalid.");
        }

        var page = requestDto.Page < 1 ? 1 : requestDto.Page;
        var pageSize = requestDto.PageSize < 1 ? 20 : requestDto.PageSize;
        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var query = _queryDbContext.ApplicationUsers
            .Where(u => u.CohortId == requestDto.CohortId)
            .OrderByDescending(u => u.Experience)
            .ThenByDescending(u => u.AmountSolved)
            .ThenBy(u => u.UserName);

        var totalUsers = await query.CountAsync(cancellationToken);

        var skip = (page - 1) * pageSize;

        var usersPage = await query
            .Skip(skip)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Experience,
                u.AmountSolved,
                u.CohortId
            })
            .ToListAsync(cancellationToken);

        var entries = usersPage
            .Select((u, index) => new UserLeaderboardEntryDto
            {
                Rank = skip + index + 1,
                UserId = u.Id,
                Username = u.UserName ?? string.Empty,
                Experience = u.Experience,
                AmountSolved = u.AmountSolved,
                CohortId = u.CohortId
            })
            .ToList();

        return new UserLeaderboardPageDto
        {
            Page = page,
            PageSize = pageSize,
            TotalUsers = totalUsers,
            Entries = entries
        };
    }
}