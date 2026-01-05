using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.DeleteItem;

public interface IDeleteItemService
{
    public Task<Result<DeleteItemResultDto, ErrorObject<string>>> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default);
}

public class DeleteItemService(
    IDeleteItemRepository deleteItemRepository
    ) : IDeleteItemService
{
    public async Task<Result<DeleteItemResultDto, ErrorObject<string>>> DeleteItemAsync(Guid itemId, CancellationToken cancellationToken = default)
    {
        return await deleteItemRepository.DeleteItemAsync(itemId, cancellationToken);
    }
}