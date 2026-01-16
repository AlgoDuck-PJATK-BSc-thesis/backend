using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.CreateEditorLayout;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CreateLayoutController : ControllerBase
{
    private readonly ICreateLayoutService _createLayoutService;

    public CreateLayoutController(ICreateLayoutService createLayoutService)
    {
        _createLayoutService = createLayoutService;
    }

    [HttpPost]
    public async Task<IActionResult> CreateLayoutAsync([FromBody] LayoutCreateDto createDto, CancellationToken cancellationToken)
    {
        return await User.GetUserId().BindAsync(async userId =>
        {
            createDto.UserId = userId;
            return await _createLayoutService.CreateLayoutAsync(createDto, cancellationToken);
        }).ToActionResultAsync();
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