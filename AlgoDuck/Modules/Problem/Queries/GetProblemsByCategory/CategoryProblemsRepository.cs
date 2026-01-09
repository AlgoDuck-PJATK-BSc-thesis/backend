using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Utilities;
using AlgoDuckShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;

public interface ICategoryProblemsRepository
{
    public Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(string categoryName);
}

public class CategoryProblemsRepository(
    ApplicationQueryDbContext dbContext,
    IAwsS3Client awsS3Client,
    ISharedProblemRepository problemRepository
) : ICategoryProblemsRepository
{
    public async Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(string categoryName)
    {
        var problemsRdb = await dbContext.Problems
            .Include(p => p.Category)
            .Include(p => p.Difficulty)
            .Where(p => p.Category.CategoryName == categoryName && p.Status == ProblemStatus.Verified)
            .Select(p => new ProblemDisplayDbPartial
            {
                ProblemId = p.ProblemId,
                Difficulty = p.Difficulty.DifficultyName,
                Tags = p.Tags.Select(t => new TagDto
                {
                    Name = t.TagName
                }).ToList()
            })
            .ToDictionaryAsync(p => p.ProblemId, p => p);

        var problemsS3 = problemsRdb.Select(async p => await problemRepository.GetProblemInfoAsync(p.Key)).ToList();

        await Task.WhenAll(problemsS3);

        return Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>.Ok(problemsS3.Select(t => t.Result)
            .Where(t => t.IsOk).Select(t => t.AsT0).Select(t => new ProblemDisplayDto
            {
                Difficulty = new DifficultyDto
                {
                    Name = problemsRdb[t.ProblemId].Difficulty
                },
                Description = t.Description,
                ProblemId = t.ProblemId,
                Tags = problemsRdb[t.ProblemId].Tags,
                Title = t.Title
            }).ToList());
    }
}

public class ProblemDisplayDbPartial
{
    public required Guid ProblemId { get; set; }
    public required string Difficulty { get; set; }
    public List<TagDto> Tags { get; set; } = [];
}