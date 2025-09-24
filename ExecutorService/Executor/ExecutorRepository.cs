using System.Net;
using System.Text;
using Dapper;
using Amazon.S3;
using Amazon.S3.Model;
using ExecutorService.Executor.Configs;
using ExecutorService.Executor.Types;
using Microsoft.Extensions.Options;
using Npgsql;

namespace ExecutorService.Executor;

public interface IExecutorRepository
{
    public Task<List<Language>> GetSupportedLanguagesAsync();
    public Task<List<TestCase>> GetTestCasesAsync(Guid exerciseId, string entrypointClassName);
    public Task<string> GetTemplateAsync(string exerciseId);
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

    public async Task<List<TestCase>> GetTestCasesAsync(Guid exerciseId, string entrypointClassName)
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
        
        return TestCase.ParseTestCases(testCasesString, entrypointClassName);
    }

    public async Task<string> GetTemplateAsync(string exerciseId)
    {
        var getRequest = new GetObjectRequest
        {
            BucketName = _s3Settings.Value.BucketName,
            Key = $"{exerciseId}/template/work.txt"
        };

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
    
}