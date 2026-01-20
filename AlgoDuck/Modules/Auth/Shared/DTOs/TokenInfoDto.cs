namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class TokenInfoDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid SessionId { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public bool IsRevoked { get; init; }
}