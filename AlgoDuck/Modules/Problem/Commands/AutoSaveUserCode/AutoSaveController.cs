using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable ConvertClosureToMethodGroup

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

[ApiController]
[Route("api/[controller]")]
public class AutoSaveController : ControllerBase
{
    private readonly IAutoSaveService _autoSaveService;

    public AutoSaveController(IAutoSaveService autoSaveService)
    {
        _autoSaveService = autoSaveService;
    }

    [HttpPost]
    public async Task<IActionResult> AutoSaveCodeAsync([FromBody] AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken)
    {
        return await User
            .GetUserId()
            .BindAsync(async idResult =>
            {
                autoSaveDto.UserId = idResult;
                return await _autoSaveService.AutoSaveCodeAsync(autoSaveDto, cancellationToken);
            }).ToActionResultAsync("autosave completed successfully");
    }
}

public class AutoSaveResultDto
{
    public required Guid ProblemId { get; set; } 
}