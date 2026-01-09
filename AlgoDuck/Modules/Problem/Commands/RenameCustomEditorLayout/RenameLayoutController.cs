using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.RenameCustomEditorLayout;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RenameLayoutController(
    IRenameLayoutService renameLayoutService
    ) : ControllerBase
{
    [HttpPut]
    public async Task<IActionResult> RenameLayoutAsync(RenameLayoutRequestDto renameRequest, CancellationToken cancellationToken)
    {
        var renameResult = await User.GetUserId().BindAsync(async userId =>
        {
            renameRequest.UserId = userId;
            return await renameLayoutService.RenameLayoutAsync(renameRequest, cancellationToken);
        });
        return renameResult.ToActionResult();
    }
}

public class RenameLayoutRequestDto
{
    public required Guid LayoutId { get; set; }
    public required string NewName { get; set; }
    internal Guid UserId { get; set; }
}


public class RenameLayoutResultDto
{
    public required Guid LayoutId { get; set; }
    public required string NewName { get; set; }
}