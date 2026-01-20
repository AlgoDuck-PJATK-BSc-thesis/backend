using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

public interface IAutoSaveRepository
{
    public Task<Result<AutoSaveResultDto, ErrorObject<string>>> UpsertSolutionSnapshotCodeAsync(AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken = default);

    public Task<Result<bool, ErrorObject<string>>> UpsertSolutionSnapshotTestingAsync(
        TestingResultSnapshotUpdate updateDto, CancellationToken cancellationToken = default);

    public Task<Result<bool, ErrorObject<string>>> DeleteSolutionSnapshotCodeAsync(DeleteAutoSaveDto deleteAutoSaveDto,
        CancellationToken cancellationToken = default);
}

public class AutoSaveRepository : IAutoSaveRepository
{
    private readonly IAwsS3Client _awsS3Client;
        private readonly ApplicationCommandDbContext _dbContext;

        public AutoSaveRepository(IAwsS3Client awsS3Client, ApplicationCommandDbContext dbContext)
        {
            _awsS3Client = awsS3Client;
            _dbContext = dbContext;
        }

        public async Task<Result<AutoSaveResultDto, ErrorObject<string>>> UpsertSolutionSnapshotCodeAsync(AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken = default)
    {
        var rowsChanged = await _dbContext.UserSolutionSnapshots
            .Where(s => s.ProblemId == autoSaveDto.ProblemId && autoSaveDto.UserId == s.UserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.CreatedAt, DateTime.UtcNow), cancellationToken);
        if (rowsChanged == 0)
        {
            await _dbContext.UserSolutionSnapshots.AddAsync(new UserSolutionSnapshot
            {
                CreatedAt = DateTime.UtcNow,
                ProblemId = autoSaveDto.ProblemId,
                UserId = autoSaveDto.UserId,
            }, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var objectPath = $"users/{autoSaveDto.UserId}/problems/autosave/{autoSaveDto.ProblemId}.xml";

         return await _awsS3Client.PostXmlObjectAsync(objectPath, autoSaveDto, cancellationToken)
             .MapToResultAsync(dto => new AutoSaveResultDto
             {
                 ProblemId = dto.ProblemId
             });
    }

    public async Task<Result<bool, ErrorObject<string>>> UpsertSolutionSnapshotTestingAsync(
        TestingResultSnapshotUpdate updateDto,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await _dbContext.CodeExecutionStatisticss
            .Include(s => s.TestingResults)
            .FirstOrDefaultAsync(s => s.ProblemId == updateDto.ProblemId && s.UserId == updateDto.UserId,
                cancellationToken);

        if (snapshot == null)
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Execution data for {updateDto.ProblemId} {updateDto.UserId} not found"));

        var allRestCases = await _dbContext.TestCases.Where(t => t.ProblemProblemId == updateDto.ProblemId)
            .ToDictionaryAsync(t => t.TestCaseId, t => t, cancellationToken: cancellationToken);
        
        if (allRestCases.Count == 0)
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Test cases not found for problem: {updateDto.ProblemId}"));

        snapshot.TestingResults.AddRange(updateDto.TestingResults.Where(tr => allRestCases.ContainsKey(tr.TestId)).Select(t => new TestingResult
        {
            CodeExecutionId = snapshot.CodeExecutionId,
            TestCaseId = t.TestId,
            IsPassed = t.IsTestPassed,
        }));
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool, ErrorObject<string>>.Ok(true);
    }

    public async Task<Result<bool, ErrorObject<string>>> DeleteSolutionSnapshotCodeAsync(
        DeleteAutoSaveDto deleteAutoSaveDto,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _dbContext.UserSolutionSnapshots
            .Where(e => e.ProblemId == deleteAutoSaveDto.ProblemId && e.UserId == deleteAutoSaveDto.UserId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
        
        var objectPath = $"users/{deleteAutoSaveDto.UserId}/problems/autosave/{deleteAutoSaveDto.ProblemId}.xml";
        
        var result = await _awsS3Client.DeleteDocumentAsync(objectPath,cancellationToken: cancellationToken);
        if (result.IsErr)
        {
            return result;
        }
        
        return rowsAffected > 0
            ? Result<bool, ErrorObject<string>>.Ok(true)
            : Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.NotFound(
                $"No autosave for ({deleteAutoSaveDto.UserId},  {deleteAutoSaveDto.ProblemId}) found"));
    }
}

public class DeleteAutoSaveDto
{
    public required Guid ProblemId { get; set; }
    public required Guid UserId { get; set; }
}