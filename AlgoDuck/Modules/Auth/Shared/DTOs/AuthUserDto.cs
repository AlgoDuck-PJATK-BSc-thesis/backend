namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class AuthUserDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailConfirmed { get; init; }
}