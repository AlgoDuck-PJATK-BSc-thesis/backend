using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllOwnedDucksPaged;


public interface IOwnedDucksService
{
    public Task<Result<PageData<OwnedDuckDto>, ErrorObject<string>>> GetOwnedItemsByTypePagedAsync(PagedRequestWithAttribution ownedItemsRequest, CancellationToken cancellationToken = default);
}

public class OwnedDucksService(
    IOwnedDucksRepository ownedDucksRepository
    ) : IOwnedDucksService
{
    public async Task<Result<PageData<OwnedDuckDto>, ErrorObject<string>>> GetOwnedItemsByTypePagedAsync(PagedRequestWithAttribution ownedItemsRequest, CancellationToken cancellationToken = default)
    {
        return await ownedDucksRepository.GetOwnedItemsByTypePagedAsync(ownedItemsRequest, cancellationToken);
    }
}
