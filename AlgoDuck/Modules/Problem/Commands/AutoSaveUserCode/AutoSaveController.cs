using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.AspNetCore.Mvc;

// ReSharper disable ConvertClosureToMethodGroup

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

[ApiController]
[Route("api/[controller]")]
public class AutoSaveController(
    IAutoSaveService autoSaveService
) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AutoSaveCodeAsync([FromBody] AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();

        if (userId.IsErr)
            return userId.ToActionResult();

        autoSaveDto.UserId = userId.AsT0;

        await autoSaveService.AutoSaveCodeAsync(autoSaveDto, cancellationToken);
        return Ok(new StandardApiResponse
        {
            Message = "Autosave completed successfully"
        });
    }
}