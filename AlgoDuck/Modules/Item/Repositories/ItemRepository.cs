using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Repositories;

public interface IItemRepository
{
    public Task<IEnumerable<ItemDto>> GetItemsAsync(int page, int pageSize);
}

public class ItemRepository(ApplicationDbContext dbContext) : IItemRepository
{
    public async Task<IEnumerable<ItemDto>> GetItemsAsync(int page, int pageSize)
    {
        return await dbContext.Items
            .Include(i => i.Rarity) .Select(i => new ItemDto(i.ItemId, i.Name, i.Description, i.Price, new RarityDto(i.Rarity.RarityName)))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}