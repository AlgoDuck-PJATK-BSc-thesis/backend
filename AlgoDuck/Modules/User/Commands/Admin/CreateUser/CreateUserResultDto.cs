namespace AlgoDuck.Modules.User.Commands.CreateUser;

public sealed class CreateUserResultDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
}