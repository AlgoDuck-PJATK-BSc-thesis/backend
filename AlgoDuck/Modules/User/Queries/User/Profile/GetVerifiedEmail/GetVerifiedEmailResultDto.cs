namespace AlgoDuck.Modules.User.Queries.User.Profile.GetVerifiedEmail;

public sealed class GetVerifiedEmailResultDto
{
    public Guid UserId { get; init; }
    public bool EmailConfirmed { get; init; }
}