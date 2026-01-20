using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using AlgoDuck.Shared.Utilities;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;

public interface ILoadAutoSaveRepository
{
    public Task<Result<AutoSaveResponseDto?, ErrorObject<string>>> TryGetLastAutoSaveAsync(AutoSaveRequestDto request,
        CancellationToken cancellationToken = default);
}

public class LoadAutoSaveRepository : ILoadAutoSaveRepository
{
    private readonly ApplicationQueryDbContext _dbContext;
    private readonly IAwsS3Client _s3Client;

    public LoadAutoSaveRepository(IAwsS3Client s3Client, ApplicationQueryDbContext dbContext)
    {
        _s3Client = s3Client;
        _dbContext = dbContext;
    }

    public async Task<Result<AutoSaveResponseDto?, ErrorObject<string>>> TryGetLastAutoSaveAsync(
        AutoSaveRequestDto request, CancellationToken cancellationToken = default)
    {
        var result = await _dbContext.UserSolutionSnapshots
            .Include(e => e.TestingSnapshotsResults)
            .FirstOrDefaultAsync(e => e.ProblemId == request.ProblemId && e.UserId == request.UserId,
                cancellationToken: cancellationToken);

        var objectPath = $"users/{request.UserId}/problems/autosave/{request.ProblemId}.xml";

        var autoSaveGetResult = await _s3Client.GetXmlObjectByPathAsync<AutoSaveDto>(objectPath, cancellationToken);
        if (autoSaveGetResult.IsErr)
            return Result<AutoSaveResponseDto?, ErrorObject<string>>.Err(autoSaveGetResult.AsT1);

        Console.WriteLine(autoSaveGetResult.AsOk!.UserCodeB64);

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