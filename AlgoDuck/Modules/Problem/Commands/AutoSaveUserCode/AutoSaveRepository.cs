using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;

public interface IAutoSaveRepository
{
    public Task<Result<bool, ErrorObject<string>>> UpsertSolutionSnapshotCodeAsync(AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken = default);

    public Task<Result<bool, ErrorObject<string>>> UpdateSolutionSnapshotTestingAsync(
        TestingResultSnapshotUpdate updateDto, CancellationToken cancellationToken = default);

    public Task<Result<bool, ErrorObject<string>>> DeleteSolutionSnapshotCodeAsync(DeleteAutoSaveDto deleteAutoSaveDto,
        CancellationToken cancellationToken = default);
}

public class AutoSaveRepository(
    IAwsS3Client awsS3Client,
    ApplicationCommandDbContext dbContext
) : IAutoSaveRepository
{
    public async Task<Result<bool, ErrorObject<string>>> UpsertSolutionSnapshotCodeAsync(AutoSaveDto autoSaveDto,
        CancellationToken cancellationToken = default)
    {
        var rowsChanged = await dbContext.UserSolutionSnapshots
            .Where(s => s.ProblemId == autoSaveDto.ProblemId && autoSaveDto.UserId == s.UserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.CreatedAt, DateTime.UtcNow), cancellationToken);

        if (rowsChanged == 0)
        {
            await dbContext.UserSolutionSnapshots.AddAsync(new UserSolutionSnapshot
            {
                CreatedAt = DateTime.UtcNow,
                ProblemId = autoSaveDto.ProblemId,
                UserId = autoSaveDto.UserId,
            }, cancellationToken);
        }

        var objectPath = $"users/{autoSaveDto.UserId}/problems/autosave/{autoSaveDto.ProblemId}.xml";

        var res = await awsS3Client.PostXmlObjectAsync(objectPath, autoSaveDto, cancellationToken);
        return res.Match<Result<bool, ErrorObject<string>>>(
            ok => Result<bool, ErrorObject<string>>.Ok(true),
            err => Result<bool, ErrorObject<string>>.Err(err));
    }

    public async Task<Result<bool, ErrorObject<string>>> UpdateSolutionSnapshotTestingAsync(
        TestingResultSnapshotUpdate updateDto,
        CancellationToken cancellationToken = default)
    {
        var snapshot = await dbContext.UserSolutionSnapshots
            .Include(s => s.TestingSnapshotsResults)
            .FirstOrDefaultAsync(s => s.ProblemId == updateDto.ProblemId && s.UserId == updateDto.UserId,
                cancellationToken);

        if (snapshot == null)
            return Result<bool, ErrorObject<string>>.Ok(false);

        var testResultsLookup = snapshot.TestingSnapshotsResults.ToDictionary(t => t.TestCaseId);

        foreach (var tr in updateDto.TestingResults)
        {
            if (testResultsLookup.TryGetValue(tr.TestCaseId, out var existingResult))
            {
                existingResult.IsPassed = tr.Passed;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return Result<bool, ErrorObject<string>>.Ok(true);
    }

    public async Task<Result<bool, ErrorObject<string>>> DeleteSolutionSnapshotCodeAsync(
        DeleteAutoSaveDto deleteAutoSaveDto,
        CancellationToken cancellationToken = default)
    {
        var rowsAffected = await dbContext.UserSolutionSnapshots
            .Where(e => e.ProblemId == deleteAutoSaveDto.ProblemId && e.UserId == deleteAutoSaveDto.UserId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
        
        var objectPath = $"users/{deleteAutoSaveDto.UserId}/problems/autosave/{deleteAutoSaveDto.ProblemId}.xml";
        
        var result = await awsS3Client.DeleteDocumentAsync(objectPath,cancellationToken: cancellationToken);
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