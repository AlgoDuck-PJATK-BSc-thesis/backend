using AlgoDuck.Modules.Item.Queries.GetFullItemDetails;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.AdminGetFullItemDetails;


[ApiController]
[Authorize(Roles = "admin")]
[Route("api/admin/item/detail")]
public class FullItemDetailsController(
    IFullItemDetailsService fullItemDetailsService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetFullItemDetailsAsync([FromQuery] Guid itemId,
        CancellationToken cancellationToken)
    {
        return await fullItemDetailsService
            .GetFullItemDetailsAsync(itemId, cancellationToken)
            .ToActionResultAsync();
    }
}

public class FullItemDetailsDto
{
    public required Guid ItemId { get; set; }
    public required string ItemName { get; set; }
    public string? ItemDescription { get; set; }
    public required DateTime CreatedOn { get; set; }
    public required Guid CreatedBy { get; set; }
    public required int Purchases { get; set; }
    public required int Price { get; set; }
    public required string ItemType { get; set; }
    public IItemTypeSpecificData ItemTypeSpecificData { get; set; } = null!;
}

public interface IItemTypeSpecificData;

public class DuckData : IItemTypeSpecificData;

public class PlantData : IItemTypeSpecificData
{
    public required byte Width { get; set; }
    public required byte Height { get; set; }
}



