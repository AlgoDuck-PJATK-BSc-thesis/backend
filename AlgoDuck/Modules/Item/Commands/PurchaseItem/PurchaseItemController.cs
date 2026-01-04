using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.PurchaseItem;

[ApiController]
[Route("/api/[controller]")]
[Authorize]
public class PurchaseItemController(
    IPurchaseItemService purchaseItemService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> PurchaseItemAsync(
        [FromBody] PurchaseRequestDto purchaseRequest,
        CancellationToken cancellationToken)
    {
        var purchaseResult = await User
            .GetUserId()
            .BindAsync(async userId =>
            {
                purchaseRequest.UserId = userId;
                return await purchaseItemService.PurchaseItemAsync(purchaseRequest, cancellationToken);
            });
        return purchaseResult.ToActionResult();
    }
}


public class PurchaseResultDto
{
    public required Guid ItemId { get; set; }
}

public class PurchaseRequestDto
{
    internal Guid UserId { get; set; }
    public required Guid ItemId { get; set; }
}