namespace AlgoDuck.Modules.User.Queries.Admin.SearchUsers;

public interface ISearchUsersHandler
{
    Task<SearchUsersResultDto> HandleAsync(SearchUsersDto query, CancellationToken cancellationToken);
}