using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.PurchaseItem;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class PurchaseController(
    IPurchaseItemService purchaseItemService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PurchaseItemAsync(
        [FromBody] PurchaseRequestDto purchaseRequest,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId.IsErr)
            return userId.ToActionResult();

        purchaseRequest.UserId = userId.AsT0;
        
        var purchaseRes = await purchaseItemService.PurchaseItemAsync(purchaseRequest, cancellationToken);
        return purchaseRes.ToActionResult();
    }
    
}



public class ItemNotFoundException(string? msg = "") : Exception(msg);
public class ItemAlreadyPurchasedException(string? msg = "") : Exception(msg);
public class NotEnoughCurrencyException(string? msg = "") : Exception(msg);


public class PurchaseResultDto
{
    public required Guid ItemId { get; set; }
}

public class PurchaseRequestDto
{
    internal Guid UserId { get; set; }
    public required Guid ItemId { get; set; }
}

public class PurchaseRequestInternalDto
{
    internal Guid RequestingUserId { get; set; }
    public required PurchaseRequestDto PurchaseRequestDto { get; set; }
}