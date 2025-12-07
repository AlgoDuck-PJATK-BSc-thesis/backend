namespace AlgoDuck.Modules.User.Queries.SearchUsers;

public interface ISearchUsersHandler
{
    Task<IReadOnlyList<SearchUsersResultDto>> HandleAsync(SearchUsersDto query, CancellationToken cancellationToken);
}