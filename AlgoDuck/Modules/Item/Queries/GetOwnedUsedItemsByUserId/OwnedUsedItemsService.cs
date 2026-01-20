using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;



public interface IOwnedItemsService
{
    public Task<Result<OwnedItemsDto, ErrorObject<string>>> GetOwnedItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class OwnedUsedItemsService(
    IOwnedItemsRepository ownedItemsRepository
) : IOwnedItemsService
{
    public async Task<Result<OwnedItemsDto, ErrorObject<string>>> GetOwnedItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await ownedItemsRepository.GetOwnedItemsByUserId(userId, cancellationToken);
    }
}