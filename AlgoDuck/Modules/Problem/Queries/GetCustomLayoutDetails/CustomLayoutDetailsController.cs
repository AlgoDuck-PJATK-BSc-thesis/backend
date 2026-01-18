using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomLayoutDetails;

[ApiController]
[Route("api/user/layout/details")]
[Authorize]
public class CustomLayoutDetailsController : ControllerBase
{
    private readonly ICustomLayoutDetailsService _customLayoutDetailsService;

    public CustomLayoutDetailsController(ICustomLayoutDetailsService customLayoutDetailsService)
    {
        _customLayoutDetailsService = customLayoutDetailsService;
    }

    [HttpPut]
    public async Task<IActionResult> GetLayoutDetailsAsync([FromQuery] Guid layoutId, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId => await _customLayoutDetailsService.SetCustomLayoutDetailsASync(
            new CustomLayoutDetailsRequestDto
            {
                LayoutId = layoutId,
                UserId = userId
            }, cancellationToken)).ToActionResultAsync();
    }
}