using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.User.Leaderboard.GetLeaderboardGlobal;

public sealed class GetLeaderboardGlobalHandler : IGetLeaderboardGlobalHandler
{
    private const string AvatarFolderPrefix = "Ducks/Outfits/";
    private const string AdminRoleName = "admin";

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

        var adminRoleId = await _queryDbContext.Set<IdentityRole<Guid>>()
            .Where(r => r.Name == AdminRoleName)
            .Select(r => r.Id)
            .SingleAsync(cancellationToken);

        var adminUserIds = _queryDbContext.Set<IdentityUserRole<Guid>>()
            .Where(ur => ur.RoleId == adminRoleId)
            .Select(ur => ur.UserId);

        var orderedUsers = _queryDbContext.ApplicationUsers
            .Where(u => !adminUserIds.Contains(u.Id))
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
                SelectedItemId = _queryDbContext.DuckOwnerships
                    .Where(p => p.UserId == u.Id && p.SelectedAsAvatar)
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
