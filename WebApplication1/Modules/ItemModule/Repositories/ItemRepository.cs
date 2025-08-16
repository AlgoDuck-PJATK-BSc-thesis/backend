using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Modules.ItemModule.DTOs;

namespace WebApplication1.Modules.ItemModule.Repositories;

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