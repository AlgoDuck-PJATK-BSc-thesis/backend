using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllOwnedDucksPaged;

[Route("api/user/item/duck")]
[Authorize]
public class OwnedDucksController : ControllerBase
{

    private readonly IOwnedDucksService _ownedDucksService;

    public OwnedDucksController(IOwnedDucksService ownedDucksService)
    {
        _ownedDucksService = ownedDucksService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllOwnedItemsByTypePagedAsync([FromQuery] int page,
        [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId => await _ownedDucksService.GetOwnedItemsByTypePagedAsync(
            new PagedRequestWithAttribution
            {
                CurrPage = page,
                PageSize = pageSize,
                UserId = userId
            }, cancellationToken)).ToActionResultAsync();
    }
}