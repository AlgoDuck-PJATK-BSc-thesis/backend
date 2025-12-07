using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

[ApiController]
[Route("api/[controller]")]
public class AutoSaveController(
    IAutoSaveService autoSaveService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> AutoSaveCodeAsync([FromBody] AutoSaveDto autoSaveDto, CancellationToken cancellationToken)
    {
        autoSaveDto.UserId = User.GetUserId();
        await autoSaveService.AutoSaveCodeAsync(autoSaveDto, cancellationToken);
        return Ok(new StandardApiResponse
        {
            Message = "Autosave completed successfully"
        });
    }
}

public interface IAutoSaveService
{
    public Task AutoSaveCodeAsync(AutoSaveDto autoSaveDto, CancellationToken cancellationToken);
}

public class AutoSaveService(
    IAutoSaveRepository autoSaveRepository
    ) : IAutoSaveService
{
    public async Task AutoSaveCodeAsync(AutoSaveDto autoSaveDto, CancellationToken cancellationToken)
    {
        await autoSaveRepository.AutoSaveCodeAsync(autoSaveDto, cancellationToken);
    }
}

public interface IAutoSaveRepository
{
    public Task AutoSaveCodeAsync(AutoSaveDto autoSaveDto, CancellationToken cancellationToken);
}

public class AutoSaveRepository(
    IAwsS3Client awsS3Client
    ) : IAutoSaveRepository
{
    public async Task AutoSaveCodeAsync(AutoSaveDto autoSaveDto, CancellationToken cancellationToken)
    {
        var objectPath = $"users/{autoSaveDto.UserId}/problems/autosave/{autoSaveDto.ProblemId}.xml";
            
        await awsS3Client.PutXmlObjectAsync(objectPath, autoSaveDto, cancellationToken);
    }
}

public class AutoSaveDto
{
    public required Guid ProblemId { get; set; }
    public required string UserCodeB64 { get; init; }
    internal Guid UserId { get; set; }
}