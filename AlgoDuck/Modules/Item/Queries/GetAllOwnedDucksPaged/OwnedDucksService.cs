using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;

namespace AlgoDuck.Modules.Item.Queries.GetAllOwnedDucksPaged;


public interface IOwnedDucksService
{
    public Task<Result<PageData<OwnedDuckDto>, ErrorObject<string>>> GetOwnedItemsByTypePagedAsync(PagedRequestWithAttribution ownedItemsRequest, CancellationToken cancellationToken = default);
}

public class OwnedDucksService : IOwnedDucksService
{
    private readonly IOwnedDucksRepository _ownedDucksRepository;

    public OwnedDucksService(IOwnedDucksRepository ownedDucksRepository)
    {
        this._ownedDucksRepository = ownedDucksRepository;
    }

    public async Task<Result<PageData<OwnedDuckDto>, ErrorObject<string>>> GetOwnedItemsByTypePagedAsync(PagedRequestWithAttribution ownedItemsRequest, CancellationToken cancellationToken = default)
    {
        return await _ownedDucksRepository.GetOwnedItemsByTypePagedAsync(ownedItemsRequest, cancellationToken);
    }
}
