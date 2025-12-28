using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;

public interface IOwnedItemsRepository
{
    public Task<Result<ICollection<ItemDto>, ErrorObject<string>>> GetOwnedItemsByUserId(Guid userId, CancellationToken cancellationToken = default);
}

public class OwnedItemsRepository(
    ApplicationQueryDbContext dbContext
) : IOwnedItemsRepository
{
    public async Task<Result<ICollection<ItemDto>, ErrorObject<string>>> GetOwnedItemsByUserId(Guid userId, CancellationToken cancellationToken = default)
    {
        
        return Result<ICollection<ItemDto>, ErrorObject<string>>.Ok(await dbContext.Items
            .Include(e => e.Purchases)
            .Where(e => e.Purchases.Select(p => p.UserId).Contains(userId))
            .Select(i => new ItemDto
            {
                ItemId = i.ItemId,
                ItemType = i.Type
            }).ToListAsync(cancellationToken: cancellationToken)); 
    }
}
