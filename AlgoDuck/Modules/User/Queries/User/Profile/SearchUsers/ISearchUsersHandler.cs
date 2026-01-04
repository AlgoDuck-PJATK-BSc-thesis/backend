namespace AlgoDuck.Modules.User.Queries.User.Profile.SearchUsers;

public interface ISearchUsersHandler
{
    Task<IReadOnlyList<SearchUsersResultDto>> HandleAsync(SearchUsersDto query, CancellationToken cancellationToken);
}