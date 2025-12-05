using AlgoDuck.Modules.Auth.Shared.DTOs;

namespace AlgoDuck.Modules.Auth.Queries.SearchUsersByEmail;

public interface ISearchUsersByEmailHandler
{
    Task<IReadOnlyList<AuthUserDto>> HandleAsync(SearchUsersByEmailDto dto, CancellationToken cancellationToken);
}