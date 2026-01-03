namespace AlgoDuck.Modules.User.Commands.AdminUpdateUser;

public sealed class AdminUpdateUserResultDto
{
    public Guid UserId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public List<string> Roles { get; init; } = new();
}