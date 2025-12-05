namespace AlgoDuck.Modules.Auth.Commands.VerifyEmail;

public sealed class VerifyEmailDto
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}