using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.DeleteLayout;
[Route("api/problem/layout")]
[ApiController]
[Authorize]
public class LayoutController : ControllerBase
{
    private readonly IDeleteLayoutService _deleteLayoutService;

    public LayoutController(IDeleteLayoutService deleteLayoutService)
    {
        _deleteLayoutService = deleteLayoutService;
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteLayoutAsync([FromQuery] Guid layoutId, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId => await _deleteLayoutService.DeleteLayoutAsync(new DeleteLayoutRequest()
        {
            LayoutId = layoutId,
            RequestingUserId = userId
        }, cancellationToken)).ToActionResultAsync();
    }
}

public class DeleteLayoutRequest
{
    public required Guid LayoutId { get; init; }
    internal Guid RequestingUserId { get; set; }
}

public class DeleteLayoutResult
{
    public required Guid LayoutId { get; init; }
}
