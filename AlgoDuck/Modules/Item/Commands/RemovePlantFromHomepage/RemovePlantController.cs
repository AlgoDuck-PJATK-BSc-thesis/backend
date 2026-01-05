using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.RemovePlantFromHomepage;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RemovePlantController(
    IRemovePlantService removePlantService
    ) : ControllerBase
{
    public async Task<IActionResult> RemovePlantFromHomepageAsync([FromQuery] Guid plantId, CancellationToken cancellationToken)
    {
        var result = await User
            .GetUserId()
            .BindAsync(async userId => await removePlantService.RemovePlantFromHomepageAsync(new RemovePlantDto
            {
                UserId = userId,
                ItemId = plantId
            }, cancellationToken));

        return result.ToActionResult();
    }
}

public class RemovePlantDto
{
    public required Guid ItemId { get; set; }
    internal Guid UserId { get; set; }
}