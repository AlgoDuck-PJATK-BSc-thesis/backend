using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemsPaged;

public interface IAllItemService
{
    public Task<PageData<ItemDto>> GetAllItemsPagedAsync(int currentPage, int pageSize, CancellationToken cancellationToken);
    
}

public class AllItemService(
    IAllItemsRepository allItemsRepository
    ) : IAllItemService
{
    public async Task<PageData<ItemDto>> GetAllItemsPagedAsync(int currentPage, int pageSize, CancellationToken cancellationToken)
    {
        return await allItemsRepository.GetAllItemsPagedAsync(currentPage, pageSize, cancellationToken);
    }
}