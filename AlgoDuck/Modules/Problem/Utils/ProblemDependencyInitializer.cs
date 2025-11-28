using System.Net.Http.Headers;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.ExecutorShared;
using AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;
using AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;
using AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;
using AlgoDuck.Modules.Problem.Queries.QueryAssistant;
using AlgoDuckShared;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using IExecutorSubmitService = AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission.IExecutorSubmitService;

namespace AlgoDuck.Modules.Problem.Utils;

internal static class ProblemDependencyInitializer
{
    internal static void Initialize(WebApplicationBuilder builder)
    {

        builder.Services.AddHttpClient("executor", client =>
        {
            client.BaseAddress =
                new Uri($"http://executor:{Environment.GetEnvironmentVariable("EXECUTOR_PORT") ?? "1337"}/api/execute");
            client.Timeout = TimeSpan.FromSeconds(60);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        
        builder.Services.AddSingleton<ChatClient>(sp =>
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            const string model = "gpt-5-nano";
            return new ChatClient(model, apiKey);
        });
        
        builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));
        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3Settings = sp.GetRequiredService<IOptions<S3Settings>>().Value;
            
            var credentials = new BasicAWSCredentials(
                Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID"),
                Environment.GetEnvironmentVariable("AWS_SECRET_ACCESS_KEY")
            );

            var config = new AmazonS3Config
            {
                RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.Region)
            };

            return new AmazonS3Client(credentials, config);
        });

        builder.Services.AddScoped<IExecutorQueryInterface, ExecutorQueryInterface>();
        builder.Services.AddScoped<IExecutorDryRunService, DryRunService>();
        
        builder.Services.AddScoped<IExecutorSubmitService, SubmitService>();
        builder.Services.AddScoped<IExecutorSubmitRepository, SubmitRepository>();
        
        builder.Services.AddScoped<IAwsS3Client, AwsS3Client>();
        builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
        
        builder.Services.AddScoped<IProblemService, ProblemService>();
        builder.Services.AddScoped<IAssistantService, AssistantService>();

        builder.Services.AddScoped<IProblemCategoriesRepository, ProblemCategoriesRepository>();
        builder.Services.AddScoped<IProblemCategoriesService, ProblemCategoriesService>();
        
        builder.Services.AddScoped<ICategoryProblemsRepository, CategoryProblemsRepository>();
        builder.Services.AddScoped<ICategoryProblemsService, CategoryProblemsService>();

    }
}