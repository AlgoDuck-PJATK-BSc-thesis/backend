namespace AlgoDuck.Modules.User.Commands.AdminCreateUser;

public sealed class AdminCreateUserResultDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
}