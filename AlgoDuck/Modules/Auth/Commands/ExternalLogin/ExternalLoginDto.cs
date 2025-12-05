namespace AlgoDuck.Modules.Auth.Commands.ExternalLogin;

public sealed class ExternalLoginDto
{
    public string Provider { get; set; } = string.Empty;
    public string ExternalUserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}