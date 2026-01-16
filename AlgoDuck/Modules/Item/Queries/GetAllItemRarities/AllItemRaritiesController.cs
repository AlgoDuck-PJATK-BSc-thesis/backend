using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetAllItemRarities;

[Authorize]
[ApiController]
[Route("api/item/rarity")]
public class AllItemRaritiesController(
    IAllItemRaritiesService allItemRaritiesService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllItemRaritiesAsync(CancellationToken cancellationToken)
    {
        return await allItemRaritiesService
            .GetAllRaritiesAsync(cancellationToken)
            .ToActionResultAsync();
    }
}

public class ItemRarityDto
{
    public required Guid RarityId { get; set; }
    public required string Name { get; set; }
}
