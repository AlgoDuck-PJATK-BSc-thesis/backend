using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;

namespace AlgoDuck.Modules.User.Queries.Admin.GetUsers;

public interface IGetUsersHandler
{
    Task<PageData<UserItemDto>> HandleAsync(GetUsersDto query, CancellationToken cancellationToken);
}