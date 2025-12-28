using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemsPaged;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ItemController(
    IAllItemService allItemService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllItemsPagedAsync(
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        return Ok(new StandardApiResponse<PageData<ItemDto>>
        {
            Body = await allItemService.GetAllItemsPagedAsync(currentPage, pageSize, User.GetUserId(), cancellationToken)
        });
    }
}