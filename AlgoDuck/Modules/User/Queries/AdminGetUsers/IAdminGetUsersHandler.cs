using AlgoDuck.Modules.User.Queries.AdminShared;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.User.Queries.AdminGetUsers;

public interface IAdminGetUsersHandler
{
    Task<PageData<AdminUserItemDto>> HandleAsync(AdminGetUsersDto query, CancellationToken cancellationToken);
}