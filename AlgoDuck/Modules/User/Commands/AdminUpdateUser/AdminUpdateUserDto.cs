namespace AlgoDuck.Modules.User.Commands.AdminUpdateUser;

public sealed class AdminUpdateUserDto
{
    public string? Username { get; init; }
    public string? Role { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
}