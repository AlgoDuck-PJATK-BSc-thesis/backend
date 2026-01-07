namespace AlgoDuck.Modules.User.Queries.User.Profile.GetVerifiedEmail;

public interface IGetVerifiedEmailHandler
{
    Task<GetVerifiedEmailResultDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}