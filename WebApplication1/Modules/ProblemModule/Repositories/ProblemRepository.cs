using System.Net;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using WebApplication1.DAL;
using WebApplication1.Modules.ProblemModule.DTOs.ProblemDtos;
using WebApplication1.Shared.Configs;

namespace WebApplication1.Modules.ProblemModule.Repositories;

public interface IProblemRepository
{
    public Task<ProblemDto> GetProblemDetailsAsync(Guid problemId);
}

public class ProblemRepository(
    ApplicationDbContext dbContext,
    IOptions<S3Settings> s3Settings,
    IAmazonS3 s3Client) : IProblemRepository
{
    private readonly IAmazonS3 _s3Client = s3Client;
    
    private readonly IOptions<S3Settings> _s3Settings = s3Settings;

    public async Task<ProblemDto> GetProblemDetailsAsync(Guid problemId)
    {
        var problemTemplate = GetTemplateAsync(problemId);
        var testCases = GetTestCasesAsync(problemId);

        var problemDto = await dbContext.Problems
            .Where(p => p.ProblemId == problemId)
            .Select(p => new ProblemDto(
                p.ProblemId,
                p.ProblemTitle,
                p.Description,
                new DifficultyDto(p.Difficulty.DifficultyName),
                new CategoryDto(p.Category.CategoryName),
                new TypeDto(p.ProblemType.Name)))
            .FirstAsync();
        
        problemDto.TemplateContents = await problemTemplate;
        var tec = await testCases;
        problemDto.TestCases = tec.Where(tc => tc.IsPublic).ToList();
        
        return problemDto;
    }
    
    private async Task<string> GetTemplateAsync(Guid exerciseId)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _s3Settings.Value.BucketName,
            Key = $"{exerciseId}/template/work.txt"
        };
        var response = await _s3Client.GetObjectAsync(getRequest);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new AmazonS3Exception($"Could not get template for {exerciseId.ToString()}");
        }
        
        var buffer = new byte[response.ContentLength];
        var totalBytesRead = 0;

        while (totalBytesRead < response.ContentLength)
        {
            var bytesRead = await response.ResponseStream.ReadAsync(buffer);
            if (bytesRead == 0) break;
            totalBytesRead += bytesRead;
        }
        return Encoding.UTF8.GetString(buffer);
    }
    
    private async Task<List<TestCaseDto>> GetTestCasesAsync(Guid exerciseId)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _s3Settings.Value.BucketName,
            Key = $"{exerciseId}/test-cases.txt"
        };
        var response = await _s3Client.GetObjectAsync(getRequest);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new AmazonS3Exception($"Could not get test cases for {exerciseId}");
        }
        
        var buffer = new byte[response.ContentLength];
        var totalBytesRead = 0;

        while (totalBytesRead < response.ContentLength)
        {
            var bytesRead = await response.ResponseStream.ReadAsync(buffer);
            if (bytesRead == 0) break;
            totalBytesRead += bytesRead;
        }

        var testCasesString = Encoding.UTF8.GetString(buffer);

        return TestCaseDto.ParseTestCases(testCasesString, "Main");
    }
}