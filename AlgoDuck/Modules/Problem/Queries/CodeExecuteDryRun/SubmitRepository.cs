using System.Text;
using System.Xml.Serialization;
using AlgoDuck.Models;
using AlgoDuck.ModelsExternal;
using AlgoDuck.Shared.Utilities;
using AlgoDuckShared;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;

public interface IExecutorSubmitRepository
{
    public Task<List<TestCaseJoined>> GetTestCasesAsync(Guid exerciseId);
    public Task<ProblemS3PartialTemplate> GetTemplateAsync(Guid exerciseId);
}

public class SubmitRepository(
    ApplicationDbContext dbContext,
    IAwsS3Client awsS3Client
    ) : IExecutorSubmitRepository
{
    public async Task<List<TestCaseJoined>> GetTestCasesAsync(Guid exerciseId)
    {
        var exerciseDbPartialTestCases =
            await dbContext.TestCases.Where(t => t.ProblemProblemId == exerciseId)
                .ToDictionaryAsync(t => t.TestCaseId, t => t);

        var exerciseS3PartialTestCases = XmlToObjectParser.ParseXmlString<TestCaseS3WrapperObject>(
                                             await awsS3Client.GetDocumentStringByPathAsync(
                                                 $"problems/{exerciseId}/test-cases.xml"))
                                         ?? throw new XmlParsingException($"problems/{exerciseId}/test-cases.xml");
        
        return exerciseS3PartialTestCases.TestCases.Select(t => new
        {
            dbTestCase = exerciseDbPartialTestCases[t.TestCaseId],
            S3TestCase = t
        }).Select(t => new TestCaseJoined()
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
    
}