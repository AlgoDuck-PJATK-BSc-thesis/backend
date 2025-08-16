using Microsoft.AspNetCore.Mvc;
using WebApplication1.Modules.ItemModule.Services;

namespace WebApplication1.Modules.ItemModule.Controllers;

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