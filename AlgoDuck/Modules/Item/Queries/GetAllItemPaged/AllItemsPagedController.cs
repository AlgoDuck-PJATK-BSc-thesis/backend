using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemPaged;

[ApiController]
[Authorize]
[Route("api/item")]
public class AllItemsPagedController(IAllItemsPagedService allItemsPagedService) :  ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllItemsPagedAsync(
        [FromQuery] ColumnFilterRequest<FetchableColumn> query,
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId => await allItemsPagedService.GetAllItemsPagedAsync(new PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>>
            {
                UserId = userId,
                CurrPage = currentPage,
                PageSize = pageSize,
                FurtherData = query
            }, cancellationToken))
            .ToActionResultAsync();
    }
}


public class ItemDto
{
    public required Guid ItemId { get; set; }
    public string? ItemName { get; set; }
    public DateTime? CreatedOn { get; set; }
    public Guid? CreatedBy { get; set; }
    public int? OwnedCount { get; set; }
    public string? Type { get; set; }
}

public enum FetchableColumn
{
    ItemId,
    ItemName,
    CreatedOn,
    CreatedBy,
    OwnedCount,
    Type
}

