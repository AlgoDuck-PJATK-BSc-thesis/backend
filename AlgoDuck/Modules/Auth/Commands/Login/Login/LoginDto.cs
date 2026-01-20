namespace AlgoDuck.Modules.Auth.Commands.Login.Login;

public sealed class LoginDto
{
    public string UserNameOrEmail { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}