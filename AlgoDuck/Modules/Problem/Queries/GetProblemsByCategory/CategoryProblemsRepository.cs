using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
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
    IAwsS3Client awsS3Client
) : ICategoryProblemsRepository
{
    public async Task<Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>> GetAllProblemsForCategoryAsync(string categoryName)
    {
        var problemsRdb = await dbContext.Problems
            .Include(p => p.Category)
            .Include(p => p.Difficulty)
            .Where(p => p.Category.CategoryName == categoryName)
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

        var problemsS3 = problemsRdb.Select(async p => await GetProblemInfoAsync(p.Key)).ToList();

        await Task.WhenAll(problemsS3);

        return Result<ICollection<ProblemDisplayDto>, ErrorObject<string>>.Ok(problemsS3.Select(t => t.Result)
            .Where(t => t.IsOk).Select(t => t.AsT0).Select(t => new ProblemDisplayDto()
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

    private async Task<Result<ProblemS3PartialInfo, ErrorObject<string>>> GetProblemInfoAsync(
        Guid problemId,
        SupportedLanguage lang = SupportedLanguage.En)
    {
        var objectPath = $"problems/{problemId}/infos/{lang.GetDisplayName().ToLowerInvariant()}.xml";
        if (!await awsS3Client.ObjectExistsAsync(objectPath))
        {
            return Result<ProblemS3PartialInfo, ErrorObject<string>>.Err(ErrorObject<string>.NotFound("not found"));
        }

        var problemInfosRaw = await awsS3Client.GetDocumentStringByPathAsync(objectPath);
        var problemInfos = XmlToObjectParser.ParseXmlString<ProblemS3PartialInfo>(problemInfosRaw)
                           ?? throw new XmlParsingException(objectPath);

        return Result<ProblemS3PartialInfo, ErrorObject<string>>.Ok(problemInfos);
    }
}

public class ProblemDisplayDbPartial
{
    public required Guid ProblemId { get; set; }
    public required string Difficulty { get; set; }
    public List<TagDto> Tags { get; set; } = [];
}