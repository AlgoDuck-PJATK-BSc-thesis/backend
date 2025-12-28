using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Queries.GetUserProfile;

public sealed record UserProfileWithRolesDto(
    UserProfileDto Profile,
    string[] Roles,
    string? PrimaryRole
);