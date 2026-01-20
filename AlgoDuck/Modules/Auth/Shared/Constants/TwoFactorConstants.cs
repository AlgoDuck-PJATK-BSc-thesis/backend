namespace AlgoDuck.Modules.Auth.Shared.Constants;

public static class TwoFactorConstants
{
    public const int CodeLength = 6;
    public const int CodeLifetimeMinutes = 10;
    public const int MaxVerificationAttempts = 5;

    public const string TwoFactorCookieName = "2fa_state";
}