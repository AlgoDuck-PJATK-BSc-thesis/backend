namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class UserByTokenDto
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
}