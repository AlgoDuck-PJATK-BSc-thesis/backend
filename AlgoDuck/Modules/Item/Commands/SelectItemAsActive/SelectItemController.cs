using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Commands.SelectItemAsActive;

[Route("api/[controller]")]
public class SelectItemController(
    ISelectItemService selectItemService
    ) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> SelectItemAsync([FromBody] SelectItemDto dto, CancellationToken cancellationToken)
    {
        var userIdResult = User.GetUserId();

        if (userIdResult.IsErr) return userIdResult.ToActionResult();
        dto.UserId = userIdResult.AsT0;

        var selectRes = await selectItemService.SelectItemAsync(dto, token: cancellationToken);
        return selectRes.ToActionResult();
    }
}

public class SelectItemDto
{
    public required Guid ItemId { get; set; }
    internal Guid UserId { get; set; }
}

public class SelectItemResultDto
{
    public required Guid ItemId { get; set; }
}