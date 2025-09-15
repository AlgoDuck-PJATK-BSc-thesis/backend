using AlgoDuck.Modules.Item.DTOs;
using AlgoDuck.Modules.Item.Repositories;

namespace AlgoDuck.Modules.Item.Services;

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


