using AlgoDuck.Models;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedDucksPaged;


public interface IOwnedDucksService
{
    public Task<Result<PageData<OwnedDuckDto>, ErrorObject<string>>> GetOwnedItemsByTypePagedAsync(OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default);
}

public class OwnedDucksService(
    IOwnedDucksRepository ownedDucksRepository
    ) : IOwnedDucksService
{
    public async Task<Result<PageData<OwnedDuckDto>, ErrorObject<string>>> GetOwnedItemsByTypePagedAsync(OwnedItemsRequest ownedItemsRequest, CancellationToken cancellationToken = default)
    {
        return await ownedDucksRepository.GetOwnedItemsByTypePagedAsync(ownedItemsRequest, cancellationToken);
    }
}
