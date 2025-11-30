using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemsPaged;


public interface IAllItemsRepository
{
    public Task<PageData<ItemDto>> GetAllItemsPagedAsync(int currentPage, int pageSize, CancellationToken cancellationToken);
}

public class AllItemsRepository(
    ApplicationQueryDbContext dbContext
    ) : IAllItemsRepository
{
    public async Task<PageData<ItemDto>> GetAllItemsPagedAsync(int currentPage, int pageSize, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}

public class ItemDto
{
    public Guid ItemId { get; set; }
}