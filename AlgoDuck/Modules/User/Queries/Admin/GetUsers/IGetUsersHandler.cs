using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.User.Queries.Admin.GetUsers;

public interface IGetUsersHandler
{
    Task<PageData<UserItemDto>> HandleAsync(GetUsersDto query, CancellationToken cancellationToken);
}