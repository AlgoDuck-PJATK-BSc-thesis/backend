using AlgoDuck.DAL;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Shared.Result;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetPreviousSolutionDataById;

public interface IGetPreviousSolutionDataRepository
{
    public Task<Result<SolutionData, ErrorUnion<NotFoundError<string>, InternalError<string>>>> GetPreviousSolutionDataAsync(PreviousSolutionRequestDto requestDto, CancellationToken cancellationToken = default);
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
    
    public async Task<Result<SolutionData, ErrorUnion<NotFoundError<string>, InternalError<string>>>> GetPreviousSolutionDataAsync(PreviousSolutionRequestDto requestDto, CancellationToken cancellationToken = default)
    {
        if (! await _dbContext.UserSolutions.AnyAsync(x => x.UserId == requestDto.UserId &&  x.SolutionId == requestDto.SolutionId, cancellationToken: cancellationToken))
            return Result<SolutionData, ErrorUnion<NotFoundError<string>, InternalError<string>>>.Err(new NotFoundError<string>($"Could not attribute solution {requestDto.SolutionId} to user {requestDto.UserId}"));


        var solutionPath = $"users/{requestDto.UserId}/solutions/{requestDto.SolutionId}.xml";
        var solutionResult = await _s3Client.GetXmlObjectByPathAsync<UserSolutionPartialS3>(solutionPath, cancellationToken);
        if (solutionResult.IsErr)
            return Result<SolutionData, ErrorUnion<NotFoundError<string>, InternalError<string>>>.Err(new InternalError<string>(solutionResult.AsErr!.Body));
        
        return Result<SolutionData, ErrorUnion<NotFoundError<string>, InternalError<string>>>.Ok(new SolutionData
        {
            SolutionId = requestDto.SolutionId,
            CodeB64 = solutionResult.AsOk!.CodeB64
        });
    }
}
