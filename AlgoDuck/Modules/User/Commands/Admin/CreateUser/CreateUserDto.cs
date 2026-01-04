namespace AlgoDuck.Modules.User.Commands.CreateUser;

public sealed class CreateUserDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Role { get; init; } = "user";
    public bool EmailVerified { get; init; } = true;
    public string? Username { get; init; }
}
