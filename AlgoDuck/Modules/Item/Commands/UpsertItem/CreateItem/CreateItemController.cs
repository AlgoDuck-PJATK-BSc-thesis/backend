using AlgoDuck.Modules.Item.Commands.CreateItem;
using AlgoDuck.Modules.Item.Commands.CreateItem.Types;
using AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem.Types;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.UpsertItem.CreateItem;

[ApiController]
[Route("/api/admin/item")]
[Authorize(Roles = "admin")]
public class CreateItemController(
    ICreateItemService createItemService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateItemAsync(
        [FromForm] CreateItemRequestDto createItemDto, 
        CancellationToken cancellation)
    {
        return await User
            .GetUserId()
            .BindAsync(async user =>
            {
                createItemDto.CreatedByUserId = user;
                return await createItemService.CreateItemAsync(createItemDto, cancellation);
            }).ToActionResultAsync();
    }    
}
