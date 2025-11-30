using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

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
        return new PageData<ItemDto>
        {
            CurrPage = currentPage,
            PageSize = pageSize,
            TotalItems = await dbContext.Items.CountAsync(cancellationToken),
            Items = await dbContext.Items
                .Skip(pageSize * (currentPage - 1))
                .Take(pageSize).Select(i => new ItemDto
                {
                    ItemId = i.ItemId
                }).ToListAsync(cancellationToken: cancellationToken),
        };
    }
}

public class ItemDto
{
    public Guid ItemId { get; set; }
}