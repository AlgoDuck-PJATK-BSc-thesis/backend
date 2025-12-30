using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Queries.GetUserProfile;

public sealed record GetUserProfileResponseDto
{
    public required UserProfileDto Profile { get; init; }
    public required string[] Roles { get; init; }
    public string? PrimaryRole { get; init; }
}