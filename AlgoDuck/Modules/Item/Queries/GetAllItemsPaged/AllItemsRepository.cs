using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemsPaged;


public interface IAllItemsRepository
{
    public Task<PageData<ItemDto>> GetAllItemsPagedAsync(int currentPage, int pageSize, Guid userId,
        CancellationToken cancellationToken);
}

public class AllItemsRepository(
    ApplicationQueryDbContext dbContext
    ) : IAllItemsRepository
{
    public async Task<PageData<ItemDto>> GetAllItemsPagedAsync(int currentPage, int pageSize, Guid userId,
        CancellationToken cancellationToken)
    {
        return new PageData<ItemDto>
        {
            CurrPage = currentPage,
            PageSize = pageSize,
            TotalItems = await dbContext.Items.CountAsync(cancellationToken),
            Items = await dbContext.Items
                .Include(i => i.Purchases).ThenInclude(p => p.User)
                .Include(i => i.Rarity)
                .Where(i => i.Purchasable)
                .Skip(pageSize * (currentPage - 1))
                .Take(pageSize).Select(i => new ItemDto
                {
                    ItemId = i.ItemId,
                    Name = i.Name,
                    Description = i.Description,
                    Price = i.Price,
                    IsOwned = i.Purchases.Any(p => p.UserId == userId),
                    ItemRarity = new  ItemRarityDto
                    {
                        RarityName = i.Rarity.RarityName,
                    }
                }).ToListAsync(cancellationToken: cancellationToken),
        };
    }
}

public class ItemDto
{
    public required Guid ItemId { get; set; }
    public required string Name { get; set; }
    public required int Price { get; set; }
    public required bool IsOwned { get; set; }
    public string? Description { get; set; }
    public required ItemRarityDto ItemRarity { get; set; }
}
public class ItemRarityDto
{
    public required string RarityName { get; set; }
}