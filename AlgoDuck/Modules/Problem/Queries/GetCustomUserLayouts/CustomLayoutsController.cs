using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomUserLayouts;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomLayoutsController(
    ICustomLayoutService customLayoutService
    ) : ControllerBase
{
    public async Task<IActionResult> GetCustomLayouts(CancellationToken cancellationToken)
    {
        var userIdResult = User.GetUserId();
        if (userIdResult.IsErr)
            return userIdResult.ToActionResult();

        var customLayoutsResult = await customLayoutService.GetCustomLayoutsAsync(userIdResult.AsT0, cancellationToken: cancellationToken);
        return customLayoutsResult.ToActionResult();
    }
}

public class LayoutDto
{
    public required Guid LayoutId { get; set; }
    public required string LayoutName { get; set; }
}