using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemPages;
public interface IAllItemsPagedService
{
    public Task<Result<PageData<ItemDto>, ErrorObject<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution itemRequest, CancellationToken cancellationToken = default);
    
}

public class AllItemsPagedService(
    IAllItemsPagedRepository allItemsPagedRepository
) : IAllItemsPagedService
{
    public async Task<Result<PageData<ItemDto>, ErrorObject<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution itemRequest, CancellationToken cancellationToken = default)
    {
        return await allItemsPagedRepository.GetAllItemsPagedAsync(itemRequest, cancellationToken);
    }
}
