using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllOwnedPlantsPaged;

[Route("api/user/item/plant")]
[Authorize]
public class OwnedPlantsController : ControllerBase
{
    private readonly IOwnedPlantsService _ownedPlantsService;

    public OwnedPlantsController(IOwnedPlantsService ownedPlantsService)
    {
        _ownedPlantsService = ownedPlantsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetOwnedPlantsPagedAsync([FromQuery] int page,
        [FromQuery] int pageSize, CancellationToken cancellationToken)
    {
        var userIdRes = User.GetUserId();
        if (userIdRes.IsErr)
            return userIdRes.ToActionResult();

        var itemsResult = await _ownedPlantsService.GetOwnedPlantsAsync(new OwnedItemsRequest
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

public class OwnedPlantDto
{
    public required Guid ItemId { get; set; }
    public required byte Width { get; set; }
    public required byte Height { get; set; }
}