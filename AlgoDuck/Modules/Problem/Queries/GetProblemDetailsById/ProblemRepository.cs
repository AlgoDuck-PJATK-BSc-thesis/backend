using AlgoDuck.DAL;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsById.ProblemDtos;
using AlgoDuckShared;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemDetailsById;

public interface IProblemRepository
{
    public Task<ProblemDto> GetProblemDetailsAsync(Guid problemId);
}

public class ProblemRepository(
    ApplicationDbContext dbContext,
    IAwsS3Client awsS3Client) : IProblemRepository
{
    public async Task<ProblemDto> GetProblemDetailsAsync(Guid problemId)
    {
        var problemTemplate = GetTemplateAsync(problemId);
        var testCases = GetTestCasesAsync(problemId);

        var problemDto = await dbContext.Problems
            .AsNoTracking()
            .Where(p => p.ProblemId == problemId)
            .Select(p => new ProblemDto(
                p.ProblemId,
                p.ProblemTitle,
                p.Description,
                new DifficultyDto(p.Difficulty!.DifficultyName),
                new CategoryDto(p.Category!.CategoryName),
                new TypeDto(p.ProblemType!.Name)))
            .FirstAsync();
        
        problemDto.TemplateContents = await problemTemplate;
        problemDto.TestCases = await testCases;
        
        return problemDto;
    }
    
    private async Task<string> GetTemplateAsync(Guid exerciseId)
    {
        return await awsS3Client.GetDocumentStringByPathAsync($"{exerciseId}/template/work.txt");
    }
    
    private async Task<List<TestCaseDto>> GetTestCasesAsync(Guid exerciseId)
    {
        var testCasesString = await awsS3Client.GetDocumentStringByPathAsync($"{exerciseId}/test-cases.txt");

        return TestCaseDto.ParseTestCases(testCasesString, "Main");
    }
}