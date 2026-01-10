using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetAllOwnedEditorLayouts;

[ApiController]
[Route("api/user/layout")]
[Authorize]
public class CustomLayoutsController : ControllerBase
{
    private readonly ICustomLayoutService _customLayoutService;

    public CustomLayoutsController(ICustomLayoutService customLayoutService)
    {
        _customLayoutService = customLayoutService;
    }

    [HttpGet]
    public async Task<IActionResult> GetCustomLayouts(CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId => await _customLayoutService.GetCustomLayoutsAsync(userId, cancellationToken))
            .ToActionResultAsync();
    }
}

public class LayoutDto
{
    public required Guid LayoutId { get; set; }
    public required string LayoutName { get; set; }
}