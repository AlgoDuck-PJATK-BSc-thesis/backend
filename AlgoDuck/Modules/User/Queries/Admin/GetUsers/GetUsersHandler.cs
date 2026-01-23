using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.Admin.GetUsers;

public sealed class GetUsersHandler : IGetUsersHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationQueryDbContext _db;

    public GetUsersHandler(IUserRepository userRepository, ApplicationQueryDbContext db)
    {
        _userRepository = userRepository;
        _db = db;
    }

    public async Task<PageData<UserItemDto>> HandleAsync(GetUsersDto query, CancellationToken cancellationToken)
    {
        var (users, total) = await _userRepository.GetPagedAsync(query.Page, query.PageSize, cancellationToken);

        var userIds = users.Select(u => u.Id).Distinct().ToArray();

        var rolePairs = userIds.Length == 0
            ? new List<(Guid UserId, string RoleName)>()
            : await _db.UserRoles
                .Where(ur => userIds.Contains(ur.UserId))
                .Join(
                    _db.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new ValueTuple<Guid, string>(ur.UserId, r.Name ?? string.Empty)
                )
                .ToListAsync(cancellationToken);

        var rolesByUser = rolePairs
            .GroupBy(x => x.Item1)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Item2)
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                    .ToArray()
            );

        var items = new List<UserItemDto>(users.Count);

        foreach (var u in users)
        {
            rolesByUser.TryGetValue(u.Id, out var roles);

            items.Add(new UserItemDto
            {
                UserId = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty,
                Roles = roles ?? Array.Empty<string>()
            });
        }

        var prev = query.Page > 1 ? query.Page - 1 : (int?)null;
        var next = query.Page * query.PageSize < total ? query.Page + 1 : (int?)null;

        return new PageData<UserItemDto>
        {
            CurrPage = query.Page,
            PageSize = query.PageSize,
            TotalItems = total,
            PrevCursor = prev,
            NextCursor = next,
            Items = items
        };
    }
}
