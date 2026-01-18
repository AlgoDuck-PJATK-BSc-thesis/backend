using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.DeleteItem;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/admin/item")]
public class DeleteItemController(
    IDeleteItemService deleteItemService
    ) : Controller
{
    [HttpDelete]
    public async Task<IActionResult> DeleteItemByIdAsync([FromQuery] Guid itemId,
        CancellationToken cancellationToken = default)
    {
        return await deleteItemService
            .DeleteItemAsync(itemId, cancellationToken)
            .ToActionResultAsync();
    }
}

public class DeleteItemResultDto
{
    public required Guid ItemId { get; set; }
}