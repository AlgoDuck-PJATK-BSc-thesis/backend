using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.DropItemAsActive;

public interface IDropItemService
{
    public Task<Result<DeselectItemResultDto, ErrorObject<string>>> DeselectItemAsync(DeselectItemDto dto, CancellationToken token = default);
}

public class DropItemService(
    IDropItemRepository dropItemRepository
    ) : IDropItemService
{
    public async Task<Result<DeselectItemResultDto, ErrorObject<string>>> DeselectItemAsync(DeselectItemDto dto, CancellationToken token = default)
    {
        return await dropItemRepository.DeselectItemAsync(dto, token);
    }
}