using System.Text;
using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.OpenApi.Extensions;
using StackExchange.Redis;


namespace AlgoDuck.Modules.Problem.Commands.CreateProblem;

public interface ICreateProblemRepository
{
    public Task<Result<Guid, ErrorObject<string>>> CreateProblemAsync(CreateProblemDto problemDto,
        CancellationToken cancellationToken = default);
}

public class CreateProblemRepository(
    ApplicationCommandDbContext dbContext,
    IAwsS3Client s3Client
) : ICreateProblemRepository
{
    public async Task<Result<Guid, ErrorObject<string>>> CreateProblemAsync(
        CreateProblemDto problemDto, CancellationToken cancellationToken = default)
    {
        Console.WriteLine(JsonSerializer.Serialize(problemDto));
        List<TestCaseS3Partial> partialTestCasesS3 = [];
        List<TestCase> partialTestCasesRdb = [];

        problemDto.TestCaseJoins.ForEach(t =>
        {
            var testCaseId = Guid.NewGuid();
            partialTestCasesS3.Add(new TestCaseS3Partial
            {
                Call = t.Call,
                Expected = t.Expected,
                Setup = t.Setup,
                TestCaseId = testCaseId
            });
            
            partialTestCasesRdb.Add(new TestCase
            {
                CallFunc = t.CallFunc ??
                           "", /*TODO: this is weird since at this point we know this is non empty since the service will resolve or return */
                Display = t.Display,
                IsPublic = t.IsPublic,
                DisplayRes = t.DisplayRes,
                TestCaseId = testCaseId
            });
        });

        var problem = new Models.Problem
        {
            ProblemTitle = problemDto.ProblemTitle,
            CreatedAt = DateTime.UtcNow,
            DifficultyId = problemDto.DifficultyId,
            TestCases = partialTestCasesRdb,
            CreatedByUserId = problemDto.CreatingUserId,
            CategoryId = problemDto.CategoryId,
            Tags = problemDto.Tags.Select(t => new Tag
            {
                TagName = t.TagName,
            }).ToList(),
        };

        await dbContext.Problems.AddAsync(problem, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        await s3Client.PostXmlObjectAsync($"problems/{problem.ProblemId}/test-cases.xml", new TestCaseS3WrapperObject
        {
            ProblemId = problem.ProblemId,
            TestCases = partialTestCasesS3
        }, cancellationToken);


        await s3Client.PostXmlObjectAsync(
            $"problems/{problem.ProblemId}/infos/{SupportedLanguage.En.GetDisplayName().ToLowerInvariant()}.xml",
            new ProblemS3PartialInfo
            {
                ProblemId = problem.ProblemId,
                CountryCode = SupportedLanguage.En,
                Description = problemDto.ProblemDescription,
                Title = problemDto.ProblemTitle
            }, cancellationToken);

        await s3Client.PostXmlObjectAsync($"problems/{problem.ProblemId}/template.xml", new ProblemS3PartialTemplate
        {
            ProblemId = problem.ProblemId,
            Template = Encoding.UTF8.GetString(Convert.FromBase64String(problemDto.TemplateB64))
        }, cancellationToken);

        return Result<Guid, ErrorObject<string>>.Ok(problem.ProblemId);
    }
}