using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Shared.Types;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Queries.Admin.SearchUsers;

public sealed class SearchUsersHandler : ISearchUsersHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ApplicationQueryDbContext _db;

    public SearchUsersHandler(IUserRepository userRepository, ApplicationQueryDbContext db)
    {
        _userRepository = userRepository;
        _db = db;
    }

    public async Task<SearchUsersResultDto> HandleAsync(SearchUsersDto query, CancellationToken cancellationToken)
    {
        var q = (query.Query ?? string.Empty).Trim();

        ApplicationUser? idUser = null;

        if (Guid.TryParse(q, out var userId))
            idUser = await _userRepository.GetByIdAsync(userId, cancellationToken);

        var (usernameUsers, usernameTotal) =
            await _userRepository.SearchByUsernamePagedAsync(q, query.UsernamePage, query.UsernamePageSize, cancellationToken);

        var (emailUsers, emailTotal) =
            await _userRepository.SearchByEmailPagedAsync(q, query.EmailPage, query.EmailPageSize, cancellationToken);

        var allIds = new HashSet<Guid>();

        if (idUser is not null)
            allIds.Add(idUser.Id);

        foreach (var u in usernameUsers)
            allIds.Add(u.Id);

        foreach (var u in emailUsers)
            allIds.Add(u.Id);

        var userIds = allIds.ToArray();

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

        UserItemDto? idMatch = null;

        if (idUser is not null)
        {
            rolesByUser.TryGetValue(idUser.Id, out var roles);

            idMatch = new UserItemDto
            {
                UserId = idUser.Id,
                Username = idUser.UserName ?? string.Empty,
                Email = idUser.Email ?? string.Empty,
                Roles = roles ?? Array.Empty<string>()
            };
        }

        IReadOnlyList<UserItemDto> MapUsers(IReadOnlyList<ApplicationUser> users)
        {
            var list = new List<UserItemDto>(users.Count);

            foreach (var u in users)
            {
                rolesByUser.TryGetValue(u.Id, out var roles);

                list.Add(new UserItemDto
                {
                    UserId = u.Id,
                    Username = u.UserName ?? string.Empty,
                    Email = u.Email ?? string.Empty,
                    Roles = roles ?? Array.Empty<string>()
                });
            }

            return list;
        }

        var usernameItems = MapUsers(usernameUsers);
        var emailItems = MapUsers(emailUsers);

        int? Prev(int page) => page > 1 ? page - 1 : null;
        int? Next(int page, int pageSize, int total) => page * pageSize < total ? page + 1 : null;

        return new SearchUsersResultDto
        {
            IdMatch = idMatch,
            Username = new PageData<UserItemDto>
            {
                CurrPage = query.UsernamePage,
                PageSize = query.UsernamePageSize,
                TotalItems = usernameTotal,
                PrevCursor = Prev(query.UsernamePage),
                NextCursor = Next(query.UsernamePage, query.UsernamePageSize, usernameTotal),
                Items = usernameItems.ToList()
            },
            Email = new PageData<UserItemDto>
            {
                CurrPage = query.EmailPage,
                PageSize = query.EmailPageSize,
                TotalItems = emailTotal,
                PrevCursor = Prev(query.EmailPage),
                NextCursor = Next(query.EmailPage, query.EmailPageSize, emailTotal),
                Items = emailItems.ToList()
            }
        };
    }
}
