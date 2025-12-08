namespace AlgoDuck.Modules.User.Queries.GetVerifiedEmail;

public interface IGetVerifiedEmailHandler
{
    Task<GetVerifiedEmailResultDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}