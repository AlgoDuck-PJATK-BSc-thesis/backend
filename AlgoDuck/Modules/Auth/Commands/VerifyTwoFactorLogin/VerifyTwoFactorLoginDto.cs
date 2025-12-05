namespace AlgoDuck.Modules.Auth.Commands.VerifyTwoFactorLogin;

public sealed class VerifyTwoFactorLoginDto
{
    public string ChallengeId { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}