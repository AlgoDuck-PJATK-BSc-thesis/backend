namespace AlgoDuck.Modules.Auth.Shared.DTOs;

public sealed class AuthResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string RefreshToken { get; init; } = string.Empty;
    public string CsrfToken { get; init; } = string.Empty;
    public DateTimeOffset AccessTokenExpiresAt { get; init; }
    public DateTimeOffset RefreshTokenExpiresAt { get; init; }
    public Guid SessionId { get; init; }
    public Guid UserId { get; init; }
}