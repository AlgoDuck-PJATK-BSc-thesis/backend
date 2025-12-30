using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.GetCohortLeaderboard;

public sealed class GetCohortLeaderboardHandler : IGetCohortLeaderboardHandler
{
    private const string AvatarFolderPrefix = "Ducks/Outfits/";

    private readonly ApplicationQueryDbContext _queryDbContext;
    private readonly IS3AvatarUrlGenerator _avatarUrlGenerator;

    public GetCohortLeaderboardHandler(ApplicationQueryDbContext queryDbContext, IS3AvatarUrlGenerator avatarUrlGenerator)
    {
        _queryDbContext = queryDbContext;
        _avatarUrlGenerator = avatarUrlGenerator;
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
                string avatarKey = string.Empty;

                if (u.SelectedItemId.HasValue)
                {
                    avatarKey = AvatarFolderPrefix + "duck-" + u.SelectedItemId.Value.ToString("D") + ".png";
                }

                var avatarUrl = _avatarUrlGenerator.GetAvatarUrl(avatarKey);

                return new UserLeaderboardEntryDto
                {
                    Rank = skip + index + 1,
                    UserId = u.Id,
                    Username = u.UserName ?? string.Empty,
                    Experience = u.Experience,
                    AmountSolved = u.AmountSolved,
                    CohortId = u.CohortId,
                    UserAvatarUrl = avatarUrl
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