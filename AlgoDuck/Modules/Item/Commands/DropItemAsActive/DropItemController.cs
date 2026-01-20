using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.DropItemAsActive;

[Route("api/[controller]")]
public class DropItemController(
    IDropItemService dropItemService
    ) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> SelectItemAsync([FromBody] DeselectItemDto dto, CancellationToken cancellationToken)
    {
        var userIdResult = User.GetUserId();

        if (userIdResult.IsErr) return userIdResult.ToActionResult();
        dto.UserId = userIdResult.AsT0;

        var selectRes = await dropItemService.DeselectItemAsync(dto, token: cancellationToken);
        return selectRes.ToActionResult();
    }
}


public class DeselectItemDto
{
    public required Guid ItemId { get; set; }
    internal Guid UserId { get; set; }
}

public class DeselectItemResultDto
{
    public required Guid ItemId { get; set; }
}