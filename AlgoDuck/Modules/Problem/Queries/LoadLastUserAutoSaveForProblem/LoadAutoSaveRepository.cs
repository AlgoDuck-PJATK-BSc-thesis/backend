using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using AlgoDuck.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;

public interface ILoadAutoSaveRepository
{
    public Task<Result<AutoSaveResponseDto?, ErrorObject<string>>> TryGetLastAutoSaveAsync(AutoSaveRequestDto request, CancellationToken cancellationToken = default);
}

public class LoadAutoSaveRepository(
    ApplicationQueryDbContext dbContext,
    IAwsS3Client s3Client
) : ILoadAutoSaveRepository
{

    public async Task<Result<AutoSaveResponseDto?, ErrorObject<string>>> TryGetLastAutoSaveAsync(AutoSaveRequestDto request, CancellationToken cancellationToken = default)
    {

        var result = await dbContext.UserSolutionSnapshots
            .Include(e => e.TestingSnapshotsResults)
            .FirstOrDefaultAsync(e => e.ProblemId == request.ProblemId && e.UserId == request.UserId,
                cancellationToken: cancellationToken);
        
        var objectPath = $"users/{request.UserId}/problems/autosave/{request.ProblemId}.xml";

        var autoSaveGetResult = await s3Client.GetXmlObjectByPathAsync<AutoSaveDto>(objectPath, cancellationToken);
        if (autoSaveGetResult.IsErr)
            return Result<AutoSaveResponseDto?, ErrorObject<string>>.Err(autoSaveGetResult.AsT1);
        
        return Result<AutoSaveResponseDto?, ErrorObject<string>>.Ok(new AutoSaveResponseDto
        {
            ProblemId = autoSaveGetResult.AsT0.ProblemId,
            UserCodeB64 = autoSaveGetResult.AsT0.UserCodeB64,
            TestResults = (result?.TestingSnapshotsResults ?? []).Select(tr => new TestResults
            {
                IsPassed = tr.IsPassed,
                TestId = tr.TestCaseId
            }).ToList()
        });
    }    
}