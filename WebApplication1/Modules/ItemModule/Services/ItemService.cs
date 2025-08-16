using WebApplication1.Modules.ItemModule.DTOs;
using WebApplication1.Modules.ItemModule.Repositories;

namespace WebApplication1.Modules.ItemModule.Services;

public interface IItemService
{
    public Task<IEnumerable<ItemDto>> GetItemsAsync(int page, int pageSize);
}

public class ItemService(IItemRepository itemRepository) : IItemService
{
    public async Task<IEnumerable<ItemDto>> GetItemsAsync(int page, int pageSize)
    {
        return await itemRepository.GetItemsAsync(page, pageSize);
    }
}


