using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.CreateItem;


public interface ICreateItemService
{
    public Task<Result<ItemCreateResponseDto, ErrorObject<string>>> CreateItemAsync(CreateItemRequestDto createItemRequestDto, CancellationToken cancellationToken = default);
}

public class CreateItemService(
    ICreateItemRepository createItemRepository
    ) : ICreateItemService
{
    public async Task<Result<ItemCreateResponseDto, ErrorObject<string>>> CreateItemAsync(CreateItemRequestDto createItemRequestDto, CancellationToken cancellationToken = default)
    {
        return await createItemRepository.CreateItemAsync(createItemRequestDto, cancellationToken);
    }
}