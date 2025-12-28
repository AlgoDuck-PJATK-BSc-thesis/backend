using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;



public interface IOwnedItemsService
{
    public Task<Result<ICollection<ItemDto>, ErrorObject<string>>> GetOwnedItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class OwnedItemsService(
    IOwnedItemsRepository ownedItemsRepository
) : IOwnedItemsService
{
    public async Task<Result<ICollection<ItemDto>, ErrorObject<string>>> GetOwnedItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ownedItemsRepository.GetOwnedItemsByUserId(userId, cancellationToken);
    }
}