using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemPaged;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AllItemsPagedController(IAllItemsPagedService allItemsPagedService) :  ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllItemsPagedAsync(
        [FromQuery] ColumnFilterRequest query,
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId => await allItemsPagedService.GetAllItemsPagedAsync(new PagedRequestWithAttribution<ColumnFilterRequest>
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

public class ColumnFilterRequest
{
    [FromQuery(Name = "columns")]
    public string? FieldsRaw { get; set; }

    public HashSet<FetchableColumn> Fields =>
        FieldsRaw?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(f => Enum.TryParse<FetchableColumn>(f, ignoreCase: true, out var val) ? val : (FetchableColumn?)null)
            .Where(f => f.HasValue)
            .Select(f => f!.Value)
            .ToHashSet()
        ?? [];
    [FromQuery(Name = "orderBy")]
    public string? OrderByRaw { get; set; }
    
    public FetchableColumn? OrderBy => Enum.TryParse<FetchableColumn>(OrderByRaw, ignoreCase: true, out var val) ? val : null;
    
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

