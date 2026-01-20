namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class UserSessionDto
{
    public Guid Id { get; init; }
    public string UserAgent { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastSeenAt { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public bool IsActive { get; init; }
}