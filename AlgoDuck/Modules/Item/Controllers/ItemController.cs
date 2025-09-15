using AlgoDuck.Modules.Item.Services;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Controllers;

[ApiController]
[Route("/api/[controller]")]
public class ItemController(IItemService itemService) : ControllerBase
{
    [HttpGet("shop")]
    public async Task<IActionResult> Async([FromQuery] int page,[FromQuery] int pageSize)
    {
        return Ok(await itemService.GetItemsAsync(page, pageSize));
    }
}