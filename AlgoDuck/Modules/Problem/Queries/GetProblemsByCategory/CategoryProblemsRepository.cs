using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Shared.Utilities;
using AlgoDuckShared;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Extensions;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;

public interface ICategoryProblemsRepository
{
    public Task<ICollection<ProblemDisplayDto>> GetAllProblemsForCategoryAsync(string categoryName);
}

public class CategoryProblemsRepository(
    ApplicationQueryDbContext dbContext,
    IAwsS3Client awsS3Client
) : ICategoryProblemsRepository
{
    public async Task<ICollection<ProblemDisplayDto>> GetAllProblemsForCategoryAsync(string categoryName)
    {
        var problemIds = await dbContext.Problems
            .Include(p => p.Category)
            .Include(p => p.Difficulty)
            .Where(p => p.Category.CategoryName == categoryName)
            .Select(p => new
            {
                ProblemId = p.ProblemId,
                Difficulty = p.Difficulty.DifficultyName,
                Tags = p.Tags
            })
            .ToListAsync();

        var tasks = problemIds.Select(async p =>
        {
            var info = await GetProblemInfoAsync(p.ProblemId);
            return new ProblemDisplayDto
            {
                ProblemId = p.ProblemId,
                Difficulty = new DifficultyDto { Name = p.Difficulty },
                Tags = p.Tags.Select(t => new TagDto { Name = t.TagName }),
                Title = info.Title
            };
        }).ToList();

        return await Task.WhenAll(tasks);
    }

    private async Task<ProblemS3PartialInfo> GetProblemInfoAsync(
        Guid problemId,
        SupportedLanguage lang = SupportedLanguage.En)
    {
        var objectPath = $"problems/{problemId}/infos/{lang.GetDisplayName().ToLowerInvariant()}.xml";
        if (!await awsS3Client.ObjectExistsAsync(objectPath))
        {
            throw new NotFoundException(objectPath);
        }

        var problemInfosRaw = await awsS3Client.GetDocumentStringByPathAsync(objectPath);
        var problemInfos = XmlToObjectParser.ParseXmlString<ProblemS3PartialInfo>(problemInfosRaw)
                           ?? throw new XmlParsingException(objectPath);

        return problemInfos;
    }
}