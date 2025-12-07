using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.SearchUsers;

public sealed class SearchUsersHandler : ISearchUsersHandler
{
    private readonly IUserRepository _userRepository;

    public SearchUsersHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<SearchUsersResultDto>> HandleAsync(
        SearchUsersDto query,
        CancellationToken cancellationToken)
    {
        var users = await _userRepository.SearchAsync(query.Query, query.Page, query.PageSize, cancellationToken);

        return users
            .Select(u => new SearchUsersResultDto
            {
                UserId = u.Id,
                Username = u.UserName ?? string.Empty,
                Email = u.Email ?? string.Empty
            })
            .ToList();
    }
}