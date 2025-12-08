namespace AlgoDuck.Modules.User.Queries.GetVerifiedEmail;

public sealed class GetVerifiedEmailResultDto
{
    public Guid UserId { get; init; }
    public bool EmailConfirmed { get; init; }
}