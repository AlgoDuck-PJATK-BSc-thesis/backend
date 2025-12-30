using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetOwnedDucksPaged;

[Route("api/[controller]")]
[Authorize]
public class OwnedDucksController(
    IOwnedDucksService ownedDucksService
) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllOwnedItemsByTypePagedAsync([FromQuery] int page,
        [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var userIdRes = User.GetUserId();
        if (userIdRes.IsErr)
            return userIdRes.ToActionResult();

        var itemsResult = await ownedDucksService.GetOwnedItemsByTypePagedAsync(new OwnedItemsRequest()
        {
            CurrPage = page,
            PageSize = pageSize,
            UserId = userIdRes.AsT0
        }, cancellationToken: cancellationToken);

        return itemsResult.ToActionResult();
    }
}

public class OwnedItemsRequest
{
    public required int CurrPage { get; set; }
    public required int PageSize { get; set; }
    public required Guid UserId { get; set; }
}

public class OwnedDuckDto
{
    public required Guid ItemId { get; set; }
    public required bool IsSelectedAsAvatar { get; set; }
    public required bool IsSelectedForPond { get; set; }
}