using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllPlantsPaged;

[ApiController]
[Route("api/item/plant")]
[Authorize]
public class AllPlantsController : ControllerBase
{
    private readonly IAllPlantsService _allPlantsService;

    public AllPlantsController(IAllPlantsService allPlantsService)
    {
        _allPlantsService = allPlantsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllItemsPagedAsync(
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var userIdRes = User.GetUserId();
        if (userIdRes.IsErr)
            return userIdRes.ToActionResult();

        var res = await _allPlantsService.GetAllPlantsPagedAsync(new PagedRequestWithAttribution()
        {
            PageSize = pageSize,
            CurrPage = currentPage,
            UserId = userIdRes.AsT0
        }, cancellationToken);

        return res.ToActionResult();
    }
}

public class PlantItemDto
{
    public required Guid ItemId { get; set; }
    public required string Name { get; set; }
    public required int Price { get; set; }
    public required bool IsOwned { get; set; }
    public required ItemRarityDto ItemRarity { get; set; }
    public required byte Width { get; set; }
    public required byte Height { get; set; }
}