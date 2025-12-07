namespace AlgoDuck.Modules.Auth.Queries.ValidateToken;

public sealed class ValidateTokenDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string? CsrfToken { get; set; }
}