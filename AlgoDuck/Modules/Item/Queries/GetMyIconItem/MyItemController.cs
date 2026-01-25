using AlgoDuck.Shared.Result;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Item.Queries.GetMyIconItem;

[ApiController]
[Authorize]
[Route("api/item/avatar")]
public class MyItemController : ControllerBase
{
    private readonly IGetMySelectedIconService _userAvatarService;

    public MyItemController(IGetMySelectedIconService userAvatarService)
    {
        _userAvatarService = userAvatarService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserAvatarAsync(CancellationToken cancellationToken)
    {
        return await User.UserIdToResult()
            .BindAsync(async userId => await _userAvatarService.GetUserAvatarAsync(userId, cancellationToken))
            .ToActionResultAsync();
    }
}
