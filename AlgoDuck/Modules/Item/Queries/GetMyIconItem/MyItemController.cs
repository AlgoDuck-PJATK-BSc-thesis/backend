using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetMyIconItem;

[ApiController]
[Authorize]
[Route("api/item/avatar")]
public class MyItemController : ControllerBase
{
    private readonly IGetMySelectedIconService _getMySelectedIconService;

    public MyItemController(IGetMySelectedIconService getMySelectedIconService)
    {
        _getMySelectedIconService = getMySelectedIconService;
    }

    [HttpGet]
    public async Task<IActionResult> GetMySelectedIconAsync(CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId =>
                await _getMySelectedIconService.GetMySelectedIconAsync(userId, cancellationToken))
            .ToActionResultAsync();
    }
}
