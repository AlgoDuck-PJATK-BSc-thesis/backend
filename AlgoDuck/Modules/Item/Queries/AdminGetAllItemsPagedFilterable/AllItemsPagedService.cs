using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.AdminGetAllItemsPagedFilterable;
public interface IAllItemsPagedService
{
    public Task<Result<PageData<ItemDto>, ErrorObject<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>> itemRequest, CancellationToken cancellationToken = default);
    
}

public class AllItemsPagedService : IAllItemsPagedService
{
    private readonly IAllItemsPagedRepository _allItemsPagedRepository;

    public AllItemsPagedService(IAllItemsPagedRepository allItemsPagedRepository)
    {
        _allItemsPagedRepository = allItemsPagedRepository;
    }

    public async Task<Result<PageData<ItemDto>, ErrorObject<string>>> GetAllItemsPagedAsync(PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>> itemRequest, CancellationToken cancellationToken = default)
    {
        return await _allItemsPagedRepository.GetAllItemsPagedAsync(itemRequest, cancellationToken);
    }
}
