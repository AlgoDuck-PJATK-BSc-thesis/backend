using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetPreviousSolutionDataById;

[ApiController]
[Authorize]
[Route("api/problem/solution")]
public class PreviousSolutionDataController : ControllerBase
{
    
    private readonly IGetPreviousSolutionDataService _getPreviousSolutionDataService;

    public PreviousSolutionDataController(IGetPreviousSolutionDataService getPreviousSolutionDataService)
    {
        _getPreviousSolutionDataService = getPreviousSolutionDataService;
    }

    [HttpGet]
    public async Task<IActionResult> GetPreviousSolutionDataAsync([FromQuery] Guid solutionId,
        CancellationToken cancellationToken = default)
    {
        return await User.GetUserId()
            .BindAsync(async userId =>
            await _getPreviousSolutionDataService.GetPreviousSolutionDataAsync(new PreviousSolutionRequestDto
            {
                SolutionId = solutionId,
                UserId = userId,
            }, cancellationToken)).ToActionResultAsync();
    }
}

public interface IGetPreviousSolutionDataService
{
    public Task<Result<SolutionData, ErrorObject<string>>> GetPreviousSolutionDataAsync(PreviousSolutionRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class GetPreviousSolutionDataService : IGetPreviousSolutionDataService
{
    private readonly IGetPreviousSolutionDataRepository _getPreviousSolutionDataRepository;

    public GetPreviousSolutionDataService(IGetPreviousSolutionDataRepository getPreviousSolutionDataRepository)
    {
        _getPreviousSolutionDataRepository = getPreviousSolutionDataRepository;
    }

    public Task<Result<SolutionData, ErrorObject<string>>> GetPreviousSolutionDataAsync(PreviousSolutionRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        return _getPreviousSolutionDataRepository.GetPreviousSolutionDataAsync(requestDto, cancellationToken);
    }
}

public interface IGetPreviousSolutionDataRepository
{
    public Task<Result<SolutionData, ErrorObject<string>>> GetPreviousSolutionDataAsync(PreviousSolutionRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class GetPreviousSolutionDataRepository : IGetPreviousSolutionDataRepository
{
    private readonly IAwsS3Client _s3Client;
    private readonly ApplicationQueryDbContext _dbContext;
    
    public GetPreviousSolutionDataRepository(IAwsS3Client s3Client, ApplicationQueryDbContext dbContext)
    {
        _s3Client = s3Client;
        _dbContext = dbContext;
    }
    
    public async Task<Result<SolutionData, ErrorObject<string>>> GetPreviousSolutionDataAsync(PreviousSolutionRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        if (! await _dbContext.UserSolutions.AnyAsync(x => x.UserId == requestDto.UserId &&  x.SolutionId == requestDto.SolutionId, cancellationToken: cancellationToken))
            return Result<SolutionData, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Could not attribute solution {requestDto.SolutionId} to user {requestDto.UserId}"));


        var solutionPath = $"users/{requestDto.UserId}/solutions/{requestDto.SolutionId}.xml";
        var solutionResult = await _s3Client.GetXmlObjectByPathAsync<UserSolutionPartialS3>(solutionPath, cancellationToken);
        if (solutionResult.IsErr)
            return Result<SolutionData, ErrorObject<string>>.Err(solutionResult.AsErr!);
        
        return Result<SolutionData, ErrorObject<string>>.Ok(new SolutionData
        {
            SolutionId = requestDto.SolutionId,
            CodeB64 = solutionResult.AsOk!.CodeB64
        });
    }
}


public class PreviousSolutionRequestDto
{
    public required Guid SolutionId { get; set; }
    internal Guid UserId { get; set; }
}

public class SolutionData
{
    public required Guid SolutionId { get; set; }
    public required string CodeB64 { get; set; }
}