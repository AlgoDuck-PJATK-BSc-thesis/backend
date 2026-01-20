using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.SelectItemAsActive;

public interface ISelectItemService
{
    public Task<Result<SelectItemResultDto, ErrorObject<string>>> SelectItemAsync(SelectItemDto item, CancellationToken token = default);
}

public class SelectItemService(
    ISelectItemRepository selectItemRepository
    ) : ISelectItemService
{
    private const int MaxSelectableDuckItemCount = 6; /*TODO: Move this to config*/
    public async Task<Result<SelectItemResultDto, ErrorObject<string>>> SelectItemAsync(SelectItemDto item, CancellationToken token = default)
    {
        var currentSelectedCount = await selectItemRepository.GetCurrentSelectedCountAsync(item, token);
        if (currentSelectedCount.IsErr)
            return Result<SelectItemResultDto, ErrorObject<string>>.Err(currentSelectedCount.AsT1);
        
        if (currentSelectedCount.AsT0 >= MaxSelectableDuckItemCount)
            return Result<SelectItemResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest($"Cannot select more than {MaxSelectableDuckItemCount}"));
        
        return await selectItemRepository.SelectItemAsync(item, token);
    }
}