using AlgoDuck.Modules.User.Queries.AdminShared;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.User.Queries.AdminSearchUsers;

public sealed class AdminSearchUsersResultDto
{
    public AdminUserItemDto? IdMatch { get; init; }
    public PageData<AdminUserItemDto> Username { get; init; } = new();
    public PageData<AdminUserItemDto> Email { get; init; } = new();
}