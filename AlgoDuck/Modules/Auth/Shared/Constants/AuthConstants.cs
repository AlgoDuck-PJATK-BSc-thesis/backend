namespace AlgoDuck.Modules.Auth.Shared.Constants;

public static class AuthConstants
{
    public const string AuthenticationScheme = "Bearer";
    public const string RefreshTokenCookieName = "refresh_token";
    public const string CsrfTokenHeaderName = "X-Csrf-Token";
    public const string CsrfTokenCookieName = "csrf_token";

    public const int MaxFailedLoginAttempts = 10;
    public const int LockoutMinutes = 15;

    public const string UserIdClaim = "sub";
    public const string UserNameClaim = "name";
    public const string EmailClaim = "email";

    public const string RoleAdmin = "Admin";
    public const string RoleModerator = "Moderator";
    public const string RoleUser = "User";
}