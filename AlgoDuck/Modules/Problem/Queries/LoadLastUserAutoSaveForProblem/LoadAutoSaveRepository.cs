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
        
        if (!await s3Client.ObjectExistsAsync(objectPath, cancellationToken)) 
            return Result<AutoSaveResponseDto?, ErrorObject<string>>.Ok(null);
        var userCodeRaw = await s3Client.GetDocumentStringByPathAsync(objectPath, cancellationToken);
        try
        {
            var autoSaveObj = XmlToObjectParser.ParseXmlString<AutoSaveDto>(userCodeRaw);
            if (autoSaveObj == null)
                return Result<AutoSaveResponseDto?, ErrorObject<string>>.Ok(null);

            return Result<AutoSaveResponseDto?, ErrorObject<string>>.Ok(new AutoSaveResponseDto()
            {
                ProblemId = autoSaveObj.ProblemId,
                UserCodeB64 = autoSaveObj.UserCodeB64,
                TestResults = (result?.TestingSnapshotsResults ?? []).Select(tr => new TestResults
                {
                    IsPassed = tr.IsPassed,
                    TestId = tr.TestCaseId
                }).ToList()
            });
        }
        catch (Exception)
        {
            return Result<AutoSaveResponseDto?, ErrorObject<string>>.Err(ErrorObject<string>.InternalError("could not retrieve template"));
        }
    }    
}
