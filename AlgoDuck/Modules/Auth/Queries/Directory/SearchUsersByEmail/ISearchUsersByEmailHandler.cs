using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Queries.Directory.SearchUsersByEmail;

public interface ISearchUsersByEmailHandler
{
    Task<IReadOnlyList<AuthUserDto>> HandleAsync(SearchUsersByEmailDto dto, CancellationToken cancellationToken);
}