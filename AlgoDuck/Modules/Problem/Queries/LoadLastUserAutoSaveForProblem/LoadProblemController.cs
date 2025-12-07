using AlgoDuck.DAL;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using AlgoDuck.Shared.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class LoadLastUserAutoSaveController(
    ILoadProblemService loadProblemService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> LoadProblemByIdAsync([FromQuery] Guid problemId, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        return Ok(new StandardApiResponse<AutoSaveDto>
        {
            Body = await loadProblemService.TryGetLastAutoSaveAsync(problemId, userId, cancellationToken)
        });
    }
}

public interface ILoadProblemService
{
    public Task<AutoSaveDto?> TryGetLastAutoSaveAsync(Guid problemId, Guid userId, CancellationToken cancellationToken);
    
}

public class LoadProblemService(
    ILoadProblemRepository loadProblemRepository
    ) : ILoadProblemService
{
    public async Task<AutoSaveDto?> TryGetLastAutoSaveAsync(Guid problemId, Guid userId, CancellationToken cancellationToken)
    {
        return await loadProblemRepository.TryGetLastAutoSaveAsync(problemId, userId, cancellationToken);
    }
}


public interface ILoadProblemRepository
{
    public Task<AutoSaveDto?> TryGetLastAutoSaveAsync(Guid problemId, Guid userId, CancellationToken cancellationToken);
}

public class LoadProblemRepository(
    ApplicationQueryDbContext dbContext,
    IAwsS3Client s3Client
    ) : ILoadProblemRepository
{

    public async Task<AutoSaveDto?> TryGetLastAutoSaveAsync(Guid problemId, Guid userId, CancellationToken cancellationToken)
    {
        var objectPath = $"users/{userId}/problems/autosave/{problemId}.xml";

        if (!await s3Client.ObjectExistsAsync(objectPath, cancellationToken)) return null;
        var userCodeRaw = await s3Client.GetDocumentStringByPathAsync(objectPath, cancellationToken);
        try
        {
            return XmlToObjectParser.ParseXmlString<AutoSaveDto>(userCodeRaw);
        }
        catch (Exception)
        {
            // ignored
        }
        return null;
    }    
}

public class AutoSaveDto
{
    public required Guid ProblemId { get; set; }
    public required string UserCodeB64 { get; init; }
}