using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.UpdateLayoutName;

[Route("api/problem/layout")]
[ApiController]
[Authorize]
public partial class LayoutController : ControllerBase
{
    private readonly IUpdateLayoutNameService _updateLayoutNameService;

    public LayoutController(IUpdateLayoutNameService updateLayoutNameService)
    {
        _updateLayoutNameService = updateLayoutNameService;
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateChatNameAsync([FromBody] RenameLayoutRequestDto request,
        CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId =>
            {
                request.UserId = userId;
                return await _updateLayoutNameService.RenameLayoutAsync(request, cancellationToken);
            })
            .ToActionResultAsync();
    }
}



public class RenameLayoutRequestDto
{
    public required Guid LayoutId { get; init; }
    public required string NewName { get; init; }
    internal Guid UserId { get; set; }
}

public class RenameLayoutResultDto
{
    public required Guid LayoutId { get; init; }
    public required string NewName { get; init; }
}