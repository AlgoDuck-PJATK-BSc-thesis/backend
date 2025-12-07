namespace AlgoDuck.Modules.Auth.Queries.GetUserByToken;

public sealed class UserByTokenDto
{
    public string AccessToken { get; set; } = string.Empty;
}