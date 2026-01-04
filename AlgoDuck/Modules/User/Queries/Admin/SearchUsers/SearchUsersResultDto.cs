using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.User.Queries.Admin.SearchUsers;

public sealed class SearchUsersResultDto
{
    public UserItemDto? IdMatch { get; init; }
    public PageData<UserItemDto> Username { get; init; } = new();
    public PageData<UserItemDto> Email { get; init; } = new();
}