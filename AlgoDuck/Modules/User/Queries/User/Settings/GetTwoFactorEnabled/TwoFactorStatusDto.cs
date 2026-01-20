namespace AlgoDuck.Modules.User.Queries.User.Settings.GetTwoFactorEnabled;

public sealed class TwoFactorStatusDto
{
    public Guid UserId { get; init; }
    public bool TwoFactorEnabled { get; init; }
}