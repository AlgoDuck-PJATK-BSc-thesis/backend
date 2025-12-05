namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class TokenValidationResult
{
    public bool IsValid { get; init; }
    public bool IsExpired { get; init; }
    public Guid? UserId { get; init; }
    public Guid? SessionId { get; init; }
    public string? FailureReason { get; init; }
}