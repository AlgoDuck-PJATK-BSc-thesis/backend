namespace AlgoDuck.Modules.User.Commands.ChangePassword;

public sealed class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}