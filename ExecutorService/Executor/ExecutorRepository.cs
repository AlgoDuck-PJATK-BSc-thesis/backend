using System.Net;
using System.Text;
using Dapper;
using ExecutorService.Executor._ExecutorUtils;

using Amazon.S3;
using Amazon.S3.Model;
using ExecutorService.Executor.Configs;
using ExecutorService.Executor.Models;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ExecutorService.Executor;

public interface IExecutorRepository
{
    public Task<List<Language>> GetSupportedLanguagesAsync();
    public Task<List<TestCase>> GetTestCasesAsync(string exerciseId);
    public Task<string> GetTemplateAsync(string exerciseId);
    public Task<string> GetFuncName(); //TODO for now will be stored in db however I'd like to add some marking mechanism to templates that indicates this is the primary "call method" to be used in testing
}

public class ExecutorRepository : IExecutorRepository
{
    private readonly IAmazonS3 _s3Client;
    private readonly IOptions<S3Settings> _s3Settings;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public ExecutorRepository(IAmazonS3 s3Client, IOptions<S3Settings> options, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _s3Settings = options;
        _configuration = configuration;
        var host = Environment.GetEnvironmentVariable("DB_HOST");
        var port = Environment.GetEnvironmentVariable("POSTGRES_PORT");
        var database = Environment.GetEnvironmentVariable("POSTGRES_DB");
        var username = Environment.GetEnvironmentVariable("POSTGRES_USER");
        var password = Environment.GetEnvironmentVariable("POSTGRES_PASSWORD");

        _connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
    }
    

    public async Task<List<Language>> GetSupportedLanguagesAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        
        const string selectLanguagesQuery = "SELECT \"Name\", \"Version\" FROM \"Languages\";";

        return (await connection.QueryAsync<Language>(selectLanguagesQuery)).ToList();
    }

    public async Task<List<TestCase>> GetTestCasesAsync(string exerciseId)
    {
        Console.WriteLine($"{exerciseId}/test-cases.txt");
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
        
        return TestCase.ParseTestCases(testCasesString);
    }

    public async Task<string> GetTemplateAsync(string exerciseId)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _s3Settings.Value.BucketName,
            Key = $"{exerciseId}/template/work.txt"
        };

        Console.WriteLine($"{exerciseId}/template/work.txt");
        var response = await _s3Client.GetObjectAsync(getRequest);

        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new AmazonS3Exception($"Could not get template for {exerciseId}");
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

    public async Task<string> GetFuncName()
    {
        return "sortIntArr";
    }
    
}

public class ExecutorRepositoryMock(IConfiguration configuration) : IExecutorRepository
{
    private IConfiguration _configuration = configuration;
    
    private readonly string _connectionString = configuration["ConnectionStrings:DefaultConnection"] ?? throw new NullReferenceException();

    public async Task<List<Language>> GetSupportedLanguagesAsync()
    {
        var executorYmlConfig = YmlConfigReader.ReadExecutorYmlConfig();

        return executorYmlConfig.SUPPORTED_LANGUAGES.ToList().Select(l => new Language(l, null)).ToList();
    }

    public async Task<List<TestCase>> GetTestCasesAsync(string exerciseId)
    {
        const string testCases = "new int[] {1,5,2,4,3}<\n" +
                                 "new int[] {1,2,3,4,5}<<\n" +
                                 "new int[] {94,37,9,52,17}<\n" +
                                 "new int[] {9,17,37,52,94}<<\n";
        return TestCase.ParseTestCases(testCases);
    }

    public async Task<string> GetTemplateAsync(string exerciseId)
    {
        return "public class Main {\n    public static int[] sortIntArr(int[] toBeSorted){\n        return null;\n    }\n}";
    }

    public async Task<string> GetFuncName()
    {
        return "sortIntArr";
    }

    /*
     proposed test case format
     test data<
     expected output<<
     test data<
     expected output<<
     ...
     Could also have them all in one line beats me, less space but less readable.
     Furthermore, enumerateTestCases would offset by 1 and 2 respectively instead of 2 and 3
     */
    
}