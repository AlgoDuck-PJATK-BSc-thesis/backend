using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin;
using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemsForCategory;

public interface ICategoryProblemsRepository
{
    public Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(
        Guid categoryId, CancellationToken cancellationToken = default);
}

public class CategoryProblemsRepository : ICategoryProblemsRepository
{
    private readonly ApplicationQueryDbContext _dbContext;
    private readonly ISharedProblemRepository _problemRepository;


    public CategoryProblemsRepository(ApplicationQueryDbContext dbContext, IAwsS3Client awsS3Client,
        ISharedProblemRepository problemRepository)
    {
        _dbContext = dbContext;
        _problemRepository = problemRepository;
    }

    public async Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(
        Guid categoryId, CancellationToken cancellationToken = default)
    {
        var problemsRdb = await _dbContext.Problems
            .Include(p => p.Difficulty)
            .Include(p => p.CodeExecutionStatistics)
            .Where(p => p.CategoryId == categoryId && p.Status == ProblemStatus.Verified)
            .Select(p => new ProblemDisplayDbPartial
            {
                ProblemId = p.ProblemId,
                Difficulty = p.Difficulty.DifficultyName,
                Tags = p.Tags.Select(t => new TagDto
                {
                    Name = t.TagName
                }).ToList(),
                RecordedAttempts = p.CodeExecutionStatistics.Select(s => new RecordedAttempt
                {
                    AttemptedAtTimestamp = s.ExecutionStartNs,
                    TestCaseResult = s.TestCaseResult
                }).OrderByDescending(s => s.AttemptedAtTimestamp).ToList()
            })
            .ToDictionaryAsync(p => p.ProblemId, p => p, cancellationToken: cancellationToken);

        var problemsS3 = problemsRdb.Select(async p => await _problemRepository.GetProblemInfoAsync(p.Key, cancellationToken: cancellationToken)).ToList();
        
        foreach (var v in problemsRdb.Values)
        {
            v.AttemptedAtTimestamp = v.RecordedAttempts.FirstOrDefault()?.AttemptedAtTimestamp.LongToDateTime();
            v.SolvedAtTimestamp = v.RecordedAttempts.FirstOrDefault(a => a.TestCaseResult == TestCaseResult.Accepted)?.AttemptedAtTimestamp.LongToDateTime();
        }
        
        await Task.WhenAll(problemsS3);

        return Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>.Ok(problemsS3
            .Where(t => t.Result.IsOk)
            .Select(t => t.Result.AsT0)
            .Select(t => new ProblemDisplayDto
            {
                Difficulty = new DifficultyDto
                {
                    Name = problemsRdb[t.ProblemId].Difficulty
                },
                Description = t.Description,
                ProblemId = t.ProblemId,
                Tags = problemsRdb[t.ProblemId].Tags,
                Title = t.Title,
                AttemptedAt = problemsRdb.TryGetValue(t.ProblemId, out var attempt) ? attempt.AttemptedAtTimestamp : null,
                SolvedAt = problemsRdb.TryGetValue(t.ProblemId, out var solution) ? solution.SolvedAtTimestamp : null,
            }).ToList());
    }
    
    private class ProblemDisplayDbPartial
    {
        internal required Guid ProblemId { get; set; }
        internal required string Difficulty { get; set; }
        internal ICollection<TagDto> Tags { get; set; } = [];
        internal ICollection<RecordedAttempt> RecordedAttempts = [];
        internal DateTime? AttemptedAtTimestamp { get; set; }
        internal DateTime? SolvedAtTimestamp { get; set; }
    }

    private class RecordedAttempt
    {
        internal required long AttemptedAtTimestamp { get; set; }
        internal required TestCaseResult TestCaseResult { get; set; }
    }
}

