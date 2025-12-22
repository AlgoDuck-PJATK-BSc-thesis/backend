using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Utilities;
using AlgoDuckShared;
using Microsoft.EntityFrameworkCore;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;

namespace AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;

public interface IExecutorSubmitRepository
{
    public Task<List<TestCaseJoined>> GetTestCasesAsync(Guid exerciseId);
    public Task<ProblemS3PartialTemplate> GetTemplateAsync(Guid exerciseId);
    public Task<Result<bool, ErrorObject<string>>> InsertSubmissionResultAsync(SubmissionInsertDto insertDto, CancellationToken cancellationToken = default);
    
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
    
    public async Task<Result<bool, ErrorObject<string>>> InsertSubmissionResultAsync(SubmissionInsertDto insertDto, CancellationToken cancellationToken = default)
    {
        var solution = new UserSolution
        {
            CodeRuntimeSubmitted = insertDto.CodeRuntimeSubmitted,
            ProblemId = insertDto.ProblemId,
            UserId = insertDto.UserId,
            Stars = 3,
            CreatedAt = DateTime.UtcNow,
        };

        try
        {
            await commandDbContext.UserSolutions.AddAsync(solution, cancellationToken);
            await commandDbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.InternalError(e.Message));
        }

        var objectPath = $"users/{solution.UserId}/problems/autosave/{solution.ProblemId}.xml";

        await awsS3Client.DeleteDocumentAsync(objectPath, cancellationToken);
        
        return await PostUserSolutionCodeToS3Async(new UserSolutionPartialS3
        {
            CodeB64 = insertDto.CodeB64,
            UserId = insertDto.UserId,
            UserSolutionId = solution.SolutionId
        });
    }

    private async Task<Result<bool, ErrorObject<string>>> PostUserSolutionCodeToS3Async(UserSolutionPartialS3 insertDto)
    {
        if (insertDto.UserId == Guid.Empty || insertDto.UserId == Guid.Empty)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest(""));
        }

        try
        {
            await awsS3Client.PostXmlObjectAsync($"users/{insertDto.UserSolutionId}/solutions/${insertDto.UserSolutionId}.xml", insertDto);
            return Result<bool, ErrorObject<string>>.Ok(true);   
        }
        catch (Exception e)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest(e.Message));
        }
    }
    
    private async Task<Result<bool, ErrorObject<string>>> DropLastCheckpointAsync(AutoSaveDropDto dropDto, CancellationToken cancellationToken = default)
    {
        var objectPath = $"users/{dropDto.UserId}/problems/autosave/{dropDto.ProblemId}.xml";
        return await awsS3Client.DeleteDocumentAsync(objectPath,  cancellationToken);
    }
}
public class SubmissionInsertDto
{
    public required long CodeRuntimeSubmitted { get; set; }
    public required Guid ProblemId { get; set; }
    public required Guid UserId { get; set; }
    public required string CodeB64 { get; set; }
}

public class UserSolutionPartialS3
{
    public required string CodeB64 { get; set; }
    internal Guid UserSolutionId { get; set; }
    internal Guid UserId { get; set; }
}

internal class AutoSaveDropDto
{
    internal required Guid UserId { get; set; }
    internal required Guid ProblemId { get; set; }
}