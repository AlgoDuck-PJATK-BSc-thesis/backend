using AlgoDuckShared;
using Dapper;
using ExecutorService.Executor.Types;
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
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;
    private readonly IAwsS3Client _awsS3Client;

    public ExecutorRepository(IAwsS3Client awsS3Client, IConfiguration configuration)
    {
        _awsS3Client = awsS3Client;
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
        var testCasesRaw = await _awsS3Client.GetDocumentStringByPathAsync($"{exerciseId}/test-cases.txt");
        
        return TestCase.ParseTestCases(testCasesRaw, entrypointClassName);
    }

    public async Task<string> GetTemplateAsync(string exerciseId)
    {
        
        return await _awsS3Client.GetDocumentStringByPathAsync($"{exerciseId}/template/work.txt");
    }
    
}