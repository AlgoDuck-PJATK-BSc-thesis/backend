namespace AlgoDuck.Modules.Auth.Commands.ResetPassword;

public sealed class ResetPasswordDto
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}