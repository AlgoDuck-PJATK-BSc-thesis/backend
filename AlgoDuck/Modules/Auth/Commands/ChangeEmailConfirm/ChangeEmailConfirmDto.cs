namespace AlgoDuck.Modules.Auth.Commands.ChangeEmailConfirm;

public sealed class ChangeEmailConfirmDto
{
    public Guid UserId { get; set; }
    public string NewEmail { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}