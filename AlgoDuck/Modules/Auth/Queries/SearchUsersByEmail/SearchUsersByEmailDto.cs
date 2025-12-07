namespace AlgoDuck.Modules.Auth.Queries.SearchUsersByEmail;

public sealed class SearchUsersByEmailDto
{
    public string Query { get; set; } = string.Empty;
    public int Limit { get; set; } = 20;
}