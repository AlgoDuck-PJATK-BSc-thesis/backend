namespace AlgoDuck.Modules.Auth.Queries.Token.ValidateToken;

public sealed class ValidateTokenResult
{
    public bool IsValid { get; set; }
    public bool IsExpired { get; set; }
    public Guid? UserId { get; set; }
    public Guid? SessionId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}