namespace AlgoDuck.Modules.Auth.Queries.Identity.GetUserByToken;

public sealed class UserByTokenDto
{
    public string AccessToken { get; set; } = string.Empty;
}