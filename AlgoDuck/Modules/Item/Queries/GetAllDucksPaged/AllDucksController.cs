using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AllDucksController(
    IAllDucksService allDucksService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllItemsPagedAsync(
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        
        var userIdRes = User.GetUserId();
        if (userIdRes.IsErr)
            return userIdRes.ToActionResult();

        var res = await allDucksService.GetAllDucksPagedAsync(new PagedRequestWithAttribution()
        {
            PageSize = pageSize,
            CurrPage = currentPage,
            UserId = userIdRes.AsT0
        }, cancellationToken);

        return res.ToActionResult();
    }
}


public class PagedRequestWithAttribution
{
    public required int CurrPage { get; set; }
    public required int PageSize { get; set; }
    public required Guid UserId { get; set; }
}


public class PagedRequestWithAttribution<TData>
{
    public required int CurrPage { get; set; }
    public required int PageSize { get; set; }
    public required Guid UserId { get; set; }
    public required TData FurtherData { get; set; }
}

public class DuckItemDto
{
    public required Guid ItemId { get; set; }
    public required string Name { get; set; }
    public required int Price { get; set; }
    public required bool IsOwned { get; set; }
    public required ItemRarityDto ItemRarity { get; set; }
}
