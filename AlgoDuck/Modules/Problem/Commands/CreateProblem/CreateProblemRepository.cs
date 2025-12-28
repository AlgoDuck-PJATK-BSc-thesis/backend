using System.Text;
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
        List<TestCaseS3Partial> partialTestCasesS3 = [];
        List<TestCase> partialTestCasesRdb = [];

        problemDto.TestCases.ForEach(t =>
        {
            var testCaseId = Guid.NewGuid();
            partialTestCasesS3.Add(new TestCaseS3Partial
            {
                Call = t.CallArgs.Select(ca => ca.Name).ToArray(),
                Expected = t.Expected.Name,
                Setup = Encoding.UTF8.GetString(Convert.FromBase64String(t.ArrangeB64)),
                TestCaseId = testCaseId
            });
            partialTestCasesRdb.Add(new TestCase
            {
                CallFunc = t.ResolvedFunctionCall ??
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
            Description = problemDto.ProblemDescription,
            CreatedAt = DateTime.UtcNow,
            DifficultyId = problemDto.DifficultyId,
            TestCases = partialTestCasesRdb,
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
                Description = problem.Description,
                Title = problemDto.ProblemTitle
            }, cancellationToken);

        return Result<Guid, ErrorObject<string>>.Ok(problem.ProblemId);
    }
}