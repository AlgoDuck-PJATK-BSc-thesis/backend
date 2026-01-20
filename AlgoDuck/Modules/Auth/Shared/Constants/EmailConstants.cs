namespace AlgoDuck.Modules.Auth.Shared.Constants;

public static class EmailConstants
{
    public const string VerificationSubject = "Verify your AlgoDuck account";
    public const string PasswordResetSubject = "Reset your AlgoDuck password";

    public const int VerificationTokenLength = 64;
    public const int PasswordResetTokenLength = 64;

    public const int VerificationTokenLifetimeMinutes = 60;
    public const int PasswordResetTokenLifetimeMinutes = 60;
}