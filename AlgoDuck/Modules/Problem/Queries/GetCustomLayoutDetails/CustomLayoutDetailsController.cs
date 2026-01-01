using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomLayoutDetails;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomLayoutDetailsController(
    ICustomLayoutDetailsService customLayoutDetailsService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLayoutDetailsAsync([FromQuery] Guid layoutId, CancellationToken cancellationToken)
    {
        var userIdResult = User.GetUserId();
        if (userIdResult.IsErr)
            return userIdResult.ToActionResult();
        var customLayoutResult = await customLayoutDetailsService.GetCustomLayoutDetailsASync(new CustomLayoutDetailsRequestDto()
        {
            UserId = userIdResult.AsT0,
            LayoutId = layoutId
        }, cancellationToken);    
        
        return customLayoutResult.ToActionResult();
    }
}

public class CustomLayoutDetailsRequestDto
{
    public required Guid LayoutId { get; set; }
    internal Guid UserId { get; set; }
}

public class CustomLayoutDetailsResponseDto
{
    public required Guid LayoutId { get; set; }
    public required object LayoutContents { get; set; }
}