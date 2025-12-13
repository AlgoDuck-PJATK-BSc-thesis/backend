using System.Net.Http.Headers;
using AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Commands.InsertTestCaseIntoUserCode;
using AlgoDuck.Modules.Problem.Commands.QueryAssistant;
using AlgoDuck.Modules.Problem.Queries.CodeExecuteDryRun;
using AlgoDuck.Modules.Problem.Queries.GetAllConversationsForProblem;
using AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;
using AlgoDuck.Modules.Problem.Queries.GetConversationsForProblem;
using AlgoDuck.Modules.Problem.Queries.GetProblemDetailsByName;
using AlgoDuck.Modules.Problem.Queries.GetProblemsByCategory;
using AlgoDuck.Modules.Problem.Queries.LoadLastUserAutoSaveForProblem;
using AlgoDuck.Shared.S3;
using AlgoDuckShared;
using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Microsoft.Extensions.Options;
using OpenAI;
using OpenAI.Chat;
using RabbitMQ.Client;
using AwsS3Client = AlgoDuck.Shared.S3.AwsS3Client;
using IAwsS3Client = AlgoDuck.Shared.S3.IAwsS3Client;
using IExecutorSubmitService = AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission.IExecutorSubmitService;

namespace AlgoDuck.Modules.Problem.Shared;

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
        
        
        builder.Services.AddSingleton<OpenAIClient>(sp =>
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            return new OpenAIClient(apiKey);
        });

        builder.Services.AddSingleton<ChatClient>(sp =>
        {
            var apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
            const string model = "gpt-5";
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
                RegionEndpoint = RegionEndpoint.GetBySystemName(s3Settings.DataBucketSettings.Region)
            };

            return new AmazonS3Client(credentials, config);
        });

        builder.Services.AddScoped<IExecutorQueryInterface, ExecutorQueryInterface>();
        builder.Services.AddScoped<IExecutorDryRunService, DryRunService>();
        
        builder.Services.AddScoped<IExecutorSubmitService, SubmitService>();
        builder.Services.AddScoped<IExecutorSubmitRepository, SubmitRepository>();
        
        builder.Services.AddScoped<IAwsS3Client, AwsS3Client>();
        builder.Services.Decorate<IAwsS3Client, AwsS3ClientCached>();
        
        builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
        builder.Services.AddScoped<IProblemService, ProblemService>();

        // builder.Services.AddScoped<IAssistantService, AssistantServiceMock>();
        builder.Services.AddScoped<IAssistantService, AssistantService>();
        builder.Services.AddScoped<IAssistantRepository, AssistantRepository>();
        
        builder.Services.AddScoped<IProblemCategoriesRepository, ProblemCategoriesRepository>();
        builder.Services.AddScoped<IProblemCategoriesService, ProblemCategoriesService>();
        
        builder.Services.AddScoped<ICategoryProblemsRepository, CategoryProblemsRepository>();
        builder.Services.AddScoped<ICategoryProblemsService, CategoryProblemsService>();

        builder.Services.AddScoped<IInsertRepository, InsertRepository>();
        builder.Services.AddScoped<IInsertService, InsertService>();

        builder.Services.AddScoped<IAutoSaveService, AutoSaveService>();
        builder.Services.AddScoped<IAutoSaveRepository, AutoSaveRepository>();

        builder.Services.AddScoped<ILoadProblemRepository, LoadProblemRepository>();
        builder.Services.AddScoped<ILoadProblemService, LoadProblemService>();

        builder.Services.AddScoped<IConversationService, ConversationService>();
        builder.Services.AddScoped<IConversationRepository, ConversationRepository>();

        builder.Services.AddScoped<IChatService, ChatService>();
        builder.Services.AddScoped<IChatRepository, ChatRepository>();

        builder.Services.AddSingleton<IConnectionFactory>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            return new ConnectionFactory
            {
                HostName = configuration["RabbitMq:HostName"] ?? "localhost",
                UserName = configuration["RabbitMq:UserName"] ?? "guest",
                Password = configuration["RabbitMq:Password"] ?? "guest",
                Port = configuration.GetValue("RabbitMq:Port", 5672),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10),
                RequestedConnectionTimeout = TimeSpan.FromSeconds(30)
            };
        });
        
        builder.Services.AddSingleton<IRabbitMqConnectionService, RabbitMqConnectionService>();
        builder.Services.AddHostedService<CodeExecutionResultChannelReadWorker>();
    }
}