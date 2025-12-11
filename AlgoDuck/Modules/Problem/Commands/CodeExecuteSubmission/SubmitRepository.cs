using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Utilities;
using AlgoDuckShared;
using Microsoft.EntityFrameworkCore;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;

public interface IExecutorSubmitRepository
{
    public Task<List<TestCaseJoined>> GetTestCasesAsync(Guid exerciseId);
    public Task<ProblemS3PartialTemplate> GetTemplateAsync(Guid exerciseId);
    public Task<bool> InsertSubmissionResultAsync(SubmissionInsertDto insertDto);
    
}

public class SubmitRepository(
    ApplicationCommandDbContext commandDbContext,
    IAwsS3Client awsS3Client
    ) : IExecutorSubmitRepository
{
    public async Task<List<TestCaseJoined>> GetTestCasesAsync(Guid exerciseId)
    {
        var exerciseDbPartialTestCases =
            await commandDbContext.TestCases.Where(t => t.ProblemProblemId == exerciseId)
                .ToDictionaryAsync(t => t.TestCaseId, t => t);

        var exerciseS3PartialTestCases = XmlToObjectParser.ParseXmlString<TestCaseS3WrapperObject>(
                                             await awsS3Client.GetDocumentStringByPathAsync(
                                                 $"problems/{exerciseId}/test-cases.xml"))
                                         ?? throw new XmlParsingException($"problems/{exerciseId}/test-cases.xml");
        
        return exerciseS3PartialTestCases.TestCases.Select(t => new
        {
            dbTestCase = exerciseDbPartialTestCases[t.TestCaseId],
            S3TestCase = t
        }).Select(t => new TestCaseJoined
        {
            Call = t.S3TestCase.Call,
            CallFunc = t.dbTestCase.CallFunc,
            Display = t.dbTestCase.Display,
            DisplayRes = t.dbTestCase.DisplayRes,
            Expected = t.S3TestCase.Expected,
            IsPublic = t.dbTestCase.IsPublic,
            ProblemProblemId = exerciseId,
            Setup = t.S3TestCase.Setup,
            TestCaseId = t.dbTestCase.TestCaseId
        }).ToList();
        
    }

    public async Task<ProblemS3PartialTemplate> GetTemplateAsync(Guid exerciseId)
    {
        return XmlToObjectParser.ParseXmlString<ProblemS3PartialTemplate>(
                   await awsS3Client.GetDocumentStringByPathAsync($"problems/{exerciseId}/template.xml")
                   ) ?? throw new XmlParsingException();
    }

    public async Task<bool> InsertSubmissionResultAsync(SubmissionInsertDto insertDto)
    {
        var userSolution = await commandDbContext.UserSolutions.AddAsync(new UserSolution
        {
            CodeRuntimeSubmitted = insertDto.ExecuteResponse.ExecutionTime,
            ProblemId = insertDto.ExecuteRequest.ProblemId,
            Stars = 3,
            StatusId = Guid.NewGuid(), /* TODO: Remember what status was supposed to be*/
            UserId = insertDto.UserId
        });

        await commandDbContext.TestingResults.AddRangeAsync(insertDto.ExecuteResponse.TestResults.Select(tr => new TestingResult
        {
            TestCaseId = Guid.Parse(tr.TestId),
            UserSolutionId = userSolution.Entity.SolutionId,
            IsPassed = tr.IsTestPassed
        }));

        await commandDbContext.SaveChangesAsync();
        await PostUserSolutionCodeToS3Async(insertDto, insertDto.UserId);
        return true;
    }

    private async Task PostUserSolutionCodeToS3Async(SubmissionInsertDto insertDto, Guid userSolutionId)
    {
        await awsS3Client.PutXmlObjectAsync($"users/{insertDto.UserId}/solutions/${insertDto.ExecuteRequest.ProblemId}/${userSolutionId}.xml", insertDto.ExecuteRequest);
    }
}
public class SubmissionInsertDto
{
    public required Guid UserId { get; set; }
    public required SubmitExecuteRequestRabbit ExecuteRequest { get; set; }
    public required SubmitExecuteResponse ExecuteResponse { get; set; } 
}