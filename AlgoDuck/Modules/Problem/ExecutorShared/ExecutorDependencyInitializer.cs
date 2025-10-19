using System.Net.Http.Headers;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsById;
using AlgoDuck.Modules.Problem.Queries.QueryAssistant;
using AlgoDuck.Shared.Configs;
using Amazon;
using Amazon.S3;
using Microsoft.Extensions.Options;
using OpenAI.Chat;

namespace AlgoDuck.Modules.Problem.ExecutorShared;

internal static class ExecutorDependencyInitializer
{
    internal static void InitializeExecutorDependencies(WebApplicationBuilder builder)
    {
        builder.Services.AddHttpClient("executor", client =>
        {
            client.BaseAddress =
                new Uri($"http://executor:{Environment.GetEnvironmentVariable("EXECUTOR_PORT") ?? "1337"}");
            client.Timeout = TimeSpan.FromSeconds(15);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        });
        
        builder.Services.AddSingleton<ChatClient>(sp =>
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            const string model = "gpt-5-nano";
            return new ChatClient(model, apiKey);
        });
        
        builder.Services.AddSingleton<IAmazonS3>(sp =>
        {
            var s3 = sp.GetRequiredService<IOptions<S3Settings>>().Value;
            var config = new AmazonS3Config { RegionEndpoint = RegionEndpoint.GetBySystemName(s3.Region) };
            return new AmazonS3Client(config);
        });

        builder.Services.AddScoped<IExecutorSubmitService, ExecutorSubmitService>();
        builder.Services.AddScoped<IExecutorDryService, ExecutorDryService>();
        builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
        builder.Services.AddScoped<IProblemService, ProblemService>();
        builder.Services.AddScoped<IAssistantService, AssistantService>();
        
    }
}