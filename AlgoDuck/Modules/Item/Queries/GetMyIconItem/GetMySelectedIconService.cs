
using AlgoDuck.Shared.Result;

namespace AlgoDuck.Modules.Item.Queries.GetMyIconItem;

public interface IGetMySelectedIconService
{
    public Task<Result<MySelectedIconDto, NotFoundError<string>>> GetUserAvatarAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetMySelectedIconService : IGetMySelectedIconService
{
    private readonly IGetMySelectedIconRepository _getMySelectedIconRepository;

    public GetMySelectedIconService(IGetMySelectedIconRepository getMySelectedIconRepository)
    {
        _getMySelectedIconRepository = getMySelectedIconRepository;
    }

    public async Task<Result<MySelectedIconDto, NotFoundError<string>>> GetUserAvatarAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _getMySelectedIconRepository.GetMySelectedIconAsync(userId, cancellationToken);
    }
}
