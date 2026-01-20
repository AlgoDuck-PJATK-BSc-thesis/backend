using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;



public interface IOwnedItemsService
{
    public Task<Result<OwnedItemsDto, ErrorObject<string>>> GetOwnedItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class OwnedUsedItemsService : IOwnedItemsService
{
    private readonly IOwnedItemsRepository _ownedItemsRepository;

    public OwnedUsedItemsService(IOwnedItemsRepository ownedItemsRepository)
    {
        _ownedItemsRepository = ownedItemsRepository;
    }

    public async Task<Result<OwnedItemsDto, ErrorObject<string>>> GetOwnedItemsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _ownedItemsRepository.GetOwnedItemsByUserId(userId, cancellationToken);
    }
}