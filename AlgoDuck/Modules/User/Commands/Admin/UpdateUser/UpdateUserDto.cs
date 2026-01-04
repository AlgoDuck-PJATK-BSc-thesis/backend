namespace AlgoDuck.Modules.User.Commands.UpdateUser;

public sealed class UpdateUserDto
{
    public string? Username { get; init; }
    public string? Role { get; init; }
    public string? Email { get; init; }
    public string? Password { get; init; }
}