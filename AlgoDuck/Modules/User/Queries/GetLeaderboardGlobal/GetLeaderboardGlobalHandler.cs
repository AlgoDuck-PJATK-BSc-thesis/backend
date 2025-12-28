using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.GetLeaderboardGlobal;

public sealed class GetLeaderboardGlobalHandler : IGetLeaderboardGlobalHandler
{
    private const string AvatarFolderPrefix = "Ducks/Outfits/";

    private readonly ApplicationQueryDbContext _queryDbContext;

    public GetLeaderboardGlobalHandler(ApplicationQueryDbContext queryDbContext)
    {
        _queryDbContext = queryDbContext;
    }

    public async Task<UserLeaderboardPageDto> HandleAsync(GetLeaderboardGlobalRequestDto requestDto, CancellationToken cancellationToken)
    {
        var page = requestDto.Page < 1 ? 1 : requestDto.Page;
        var pageSize = requestDto.PageSize < 1 ? 20 : requestDto.PageSize;
        if (pageSize > 100)
        {
            pageSize = 100;
        }

        var orderedUsers = _queryDbContext.ApplicationUsers
            .OrderByDescending(u => u.Experience)
            .ThenByDescending(u => u.AmountSolved)
            .ThenBy(u => u.UserName);

        var totalUsers = await orderedUsers.CountAsync(cancellationToken);

        var skip = (page - 1) * pageSize;

        var usersPage = await orderedUsers
            .Skip(skip)
            .Take(pageSize)
            .Select(u => new
            {
                u.Id,
                u.UserName,
                u.Experience,
                u.AmountSolved,
                u.CohortId,
                SelectedItemId = _queryDbContext.Purchases
                    .Where(p => p.UserId == u.Id && p.Selected)
                    .Select(p => (Guid?)p.ItemId)
                    .FirstOrDefault()
            })
            .ToListAsync(cancellationToken);

        var entries = usersPage
            .Select((u, index) =>
            {
                string? avatarPath = null;

                if (u.SelectedItemId.HasValue)
                {
                    avatarPath = AvatarFolderPrefix + "duck-" + u.SelectedItemId.Value.ToString("D") + ".png";
                }

                return new UserLeaderboardEntryDto
                {
                    Rank = skip + index + 1,
                    UserId = u.Id,
                    Username = u.UserName ?? string.Empty,
                    Experience = u.Experience,
                    AmountSolved = u.AmountSolved,
                    CohortId = u.CohortId,
                    UserAvatarUrl = avatarPath
                };
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
