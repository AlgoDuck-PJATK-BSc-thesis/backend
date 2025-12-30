using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.PurchaseItem;

public interface IPurchaseItemService
{
    public Task<Result<PurchaseResultDto, ErrorObject<string>>> PurchaseItemAsync(PurchaseRequestDto purchaseRequest, CancellationToken cancellationToken = default);
}

public class PurchaseItemService(
    IPurchaseItemRepository purchaseItemRepository
) : IPurchaseItemService
{
    public async Task<Result<PurchaseResultDto, ErrorObject<string>>> PurchaseItemAsync(PurchaseRequestDto purchaseRequest, CancellationToken cancellationToken = default)
    {
        var existenceCheckResult = await purchaseItemRepository.CheckIfItemExist(purchaseRequest, cancellationToken);
        if (existenceCheckResult.IsErr)
            return Result<PurchaseResultDto, ErrorObject<string>>.Err(existenceCheckResult.AsT1);
        
        return await purchaseItemRepository.PurchaseItemAsync(purchaseRequest, cancellationToken);
    }
}
