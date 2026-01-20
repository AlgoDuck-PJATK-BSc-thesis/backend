using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetUserPreviewData;

public interface IGetUserPreviewService
{
    public Task<Result<UserPreviewDto, ErrorObject<string>>> GetUserPreviewAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetUserPreviewService : IGetUserPreviewService
{
    private readonly IGetUserPreviewRepository _getUserPreviewRepository;

    public GetUserPreviewService(IGetUserPreviewRepository getUserPreviewRepository)
    {
        _getUserPreviewRepository = getUserPreviewRepository;
    }

    public async Task<Result<UserPreviewDto, ErrorObject<string>>> GetUserPreviewAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _getUserPreviewRepository.GetUserPreviewAsync(userId, cancellationToken);
    }
}
