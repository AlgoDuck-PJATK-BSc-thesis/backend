namespace AlgoDuck.Modules.Auth.Queries.GetUserSessions;

public sealed class UserSessionDto
{
    public Guid SessionId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? RevokedAt { get; set; }
    public bool IsCurrent { get; set; }
}