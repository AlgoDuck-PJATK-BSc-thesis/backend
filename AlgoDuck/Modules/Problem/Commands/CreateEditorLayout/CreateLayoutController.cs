using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.CreateEditorLayout;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CreateLayoutController(
    ICreateLayoutService createLayoutService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateLayoutAsync([FromBody] LayoutCreateDto createDto, CancellationToken cancellationToken)
    {
        var userIdResult = User.GetUserId();
        if (userIdResult.IsErr)
            return userIdResult.ToActionResult();
        
        createDto.UserId = userIdResult.AsT0;
        
        var createResult = await createLayoutService.CreateLayoutAsync(createDto, cancellationToken);
        return createResult.ToActionResult();
    }
}

public class LayoutCreateDto
{
    public required string LayoutContent { get; set; }
    public required string LayoutName { get; set; }
    internal Guid UserId { get; set; }
}

public class LayoutCreateResultDto
{
    public required Guid LayoutId { get; set; } 
}