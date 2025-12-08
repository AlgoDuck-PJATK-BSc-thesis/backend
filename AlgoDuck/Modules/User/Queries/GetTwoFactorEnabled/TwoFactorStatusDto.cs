namespace AlgoDuck.Modules.User.Queries.GetTwoFactorEnabled;

public sealed class TwoFactorStatusDto
{
    public Guid UserId { get; init; }
    public bool TwoFactorEnabled { get; init; }
}