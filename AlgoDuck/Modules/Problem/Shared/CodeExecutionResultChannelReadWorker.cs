using System.Text;
using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Commands.AutoSaveUserCode;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Shared.Repositories;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

// ReSharper disable ConvertIfStatementToReturnStatement
// ReSharper disable InvertIf

namespace AlgoDuck.Modules.Problem.Shared;

public sealed class CodeExecutionResultChannelReadWorker : BackgroundService, IAsyncDisposable
{
    private readonly IRabbitMqConnectionService _rabbitMqConnectionService;
    private readonly IHubContext<ExecutionStatusHub> _hubContext;
    private readonly ILogger<CodeExecutionResultChannelReadWorker> _logger;
    private readonly IOptionsMonitor<MessageQueuesConfig> _messageQueuesConfig;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDatabase _redis;

    private IChannel? _channel;

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CodeExecutionResultChannelReadWorker(IRabbitMqConnectionService rabbitMqConnectionService,
        IHubContext<ExecutionStatusHub> hubContext, ILogger<CodeExecutionResultChannelReadWorker> logger,
        IOptionsMonitor<MessageQueuesConfig> messageQueuesConfig, IServiceScopeFactory scopeFactory, IDatabase redis)
    {
        _rabbitMqConnectionService = rabbitMqConnectionService;
        _hubContext = hubContext;
        _logger = logger;
        _messageQueuesConfig = messageQueuesConfig;
        _scopeFactory = scopeFactory;
        _redis = redis;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await DeclareQueues(stoppingToken);

        if (_channel == null)
        {
            _logger.LogInformation("{name}: Startup failed. Read channel is null", GetType().Name);
            return;
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);

        _logger.LogInformation("{name}: Startup completed.", GetType().Name);

        consumer.ReceivedAsync += async (sender, ea) => await HandleChannelReadAsync(sender, ea, stoppingToken);

        await _channel.BasicConsumeAsync(
            queue: _messageQueuesConfig.CurrentValue.Execution.Read,
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task DeclareQueues(CancellationToken cancellationToken = default)
    {
        var connection = await _rabbitMqConnectionService.GetConnection(cancellationToken);

        _channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await _channel.QueueDeclareAsync(queue: _messageQueuesConfig.CurrentValue.Execution.Read,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    private async Task HandleChannelReadAsync(object sender, BasicDeliverEventArgs deliverEventArgs,
        CancellationToken cancellationToken = default)
    {
        if (_channel == null) return;
        var readResult = ParseChannelOutput(deliverEventArgs);

        if (readResult.IsErr)
        {
            switch (readResult.AsT1.Type)
            {
                case ErrorType.NotFound or ErrorType.BadRequest:
                    await _channel.BasicNackAsync(deliverEventArgs.DeliveryTag, multiple: false, requeue: true,
                        cancellationToken: cancellationToken);
                    return;
            }
        }

        if (readResult.IsErr) // internal error. unknown Exception, nothing we can do but skip
            return;

        var channelResponse = readResult.AsT0;

        var jobDataResult = await GetJobDataFromCacheAsync(channelResponse.JobId, cancellationToken);
        if (jobDataResult.IsErr)
            return;

        var jobData = jobDataResult.AsT0;

        var fileHelper = new ExecutorFileOperationHelper
        {
            UserSolutionData = new UserSolutionData
            {
                SigningKey = jobDataResult.AsT0.SigningKey
            }
        };
        var result = fileHelper.ParseVmOutput(channelResponse);
        jobData.CachedResponses.Add(result);

        await _redis.StringSetAsync(
            key: new RedisKey(jobData.JobId.ToString()),
            value: JsonSerializer.Serialize(jobData),
            expiry: TimeSpan.FromMinutes(3));

        await _hubContext.Clients.Group(channelResponse.JobId.ToString())
            .SendAsync(
                method: "ExecutionStatusUpdated",
                arg1: new StandardApiResponse<SubmitExecuteResponse>
                {
                    Body = result
                },
                cancellationToken: cancellationToken
            );

        await _channel.BasicAckAsync(deliverEventArgs.DeliveryTag, multiple: false,
            cancellationToken: cancellationToken);

        if (result.Status.IsIntermediateStatus()) return;

        var handleResult = await HandleCodeExecutionResultAsync(result, jobData, cancellationToken);

        if (handleResult.IsErr)
        {
            _logger.LogError("{errorType}: {errorBody}", handleResult.AsT1.Type, handleResult.AsT1.Body);
        }
    }

    private async Task<Result<bool, ErrorObject<string>>> HandleCodeExecutionResultAsync(
        SubmitExecuteResponse submitResponse, ExecutionQueueJobData jobData,
        CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var statisticsRepository = scope.ServiceProvider.GetRequiredService<IExecutionStatisticsRepository>();
        var testCaseValidationRepository = scope.ServiceProvider.GetRequiredService<ISharedTestCaseRepository>();

        var allTestsPassedResult = jobData.ProblemId is not null
            ? await testCaseValidationRepository.ValidateAllTestCasesPassedAsync(new ValidationRequestDto
            {
                ProblemId = (Guid)jobData.ProblemId!,
                TestingResults = submitResponse.TestResults.ToDictionary(t => t.TestId, t => t.IsTestPassed)
            }, cancellationToken)
            : Result<bool, ErrorObject<string>>.Ok(false);

        if (allTestsPassedResult.IsErr)
            return Result<bool, ErrorObject<string>>.Err(allTestsPassedResult.AsT1!);

        var allTestsPassed = allTestsPassedResult.AsOk;
        await statisticsRepository.RecordExecutionDataAsync(BuildExecutionData(jobData, submitResponse, allTestsPassed),
            cancellationToken);

        if (jobData.JobType == JobType.DryRun)
            return Result<bool, ErrorObject<string>>.Ok(false);

        if (jobData.ProblemId == null)
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("problem ID missing"));

        var problemId = jobData.ProblemId.Value;
        var autoSaveRepository = scope.ServiceProvider.GetRequiredService<IAutoSaveRepository>();

        if (!allTestsPassed)
            return await autoSaveRepository.UpsertSolutionSnapshotTestingAsync(new TestingResultSnapshotUpdate
            {
                ProblemId = problemId,
                UserId = jobData.UserId,
                TestingResults = submitResponse.TestResults
            }, cancellationToken);

        var submitRepository = scope.ServiceProvider.GetRequiredService<IExecutorSubmitRepository>();

        var coinAddResult = await submitRepository.AddCoinsAndExperienceAsync(new SolutionRewardDto
        {
            ProblemId = problemId,
            UserId = jobData.UserId
        }, cancellationToken);

        if (coinAddResult.IsErr)
            return Result<bool, ErrorObject<string>>.Err(coinAddResult.AsT1);

        var result = await submitRepository.InsertSubmissionResultAsync(new SubmissionInsertDto
        {
            CodeB64 = jobData.UserCodeB64,
            CodeRuntimeSubmitted = submitResponse.ExecutionTimeNs,
            ProblemId = problemId,
            UserId = jobData.UserId
        }, cancellationToken);


        if (result.IsErr)
            return result;

        return await autoSaveRepository.DeleteSolutionSnapshotCodeAsync(new DeleteAutoSaveDto
        {
            ProblemId = problemId,
            UserId = jobData.UserId
        }, cancellationToken);
    }

    private Result<ExecutionResponseRabbit, ErrorObject<string>> ParseChannelOutput(
        BasicDeliverEventArgs deliverEventArgs)
    {
        var body = deliverEventArgs.Body;
        try
        {
            var message = Encoding.UTF8.GetString(body.ToArray());
            var response = JsonSerializer.Deserialize<ExecutionResponseRabbit>(message, _jsonSerializerOptions);
            if (response == null)
            {
                _logger.LogWarning("{ReadWorkerName}: Read null message from channel {channelName}", GetType().Name,
                    _messageQueuesConfig.CurrentValue.Execution.Read);
                return Result<ExecutionResponseRabbit, ErrorObject<string>>.Err(
                    ErrorObject<string>.NotFound("Null value"));
            }

            return Result<ExecutionResponseRabbit, ErrorObject<string>>.Ok(response);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "{ReadWorkerName} Error reading job data. Invalid format", GetType().Name);
            return Result<ExecutionResponseRabbit, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest("Bad format"));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{ReadWorkerName} Error reading job data.", GetType().Name);
            return Result<ExecutionResponseRabbit, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError("Error reading job data"));
        }
    }

    private async Task<Result<ExecutionQueueJobData, ErrorObject<string>>> GetJobDataFromCacheAsync(Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var jobDataRaw = await _redis.StringGetAsync(new RedisKey(jobId.ToString()));
        if (jobDataRaw.IsNullOrEmpty)
        {
            _logger.LogWarning("{ReadWorkerName} received no jobData in cache for job: {jobId}", GetType().Name, jobId);
            return Result<ExecutionQueueJobData, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound("No jobData in cache for job: {jobId}"));
        }

        try
        {
            var jobData =
                JsonSerializer.Deserialize<ExecutionQueueJobData>(jobDataRaw.ToString(), _jsonSerializerOptions);
            if (jobData == null)
            {
                return Result<ExecutionQueueJobData, ErrorObject<string>>.Err(
                    ErrorObject<string>.InternalError("Error reading job data from cache. REASON: null value"));
            }

            return Result<ExecutionQueueJobData, ErrorObject<string>>.Ok(jobData);
        }
        catch (JsonException e)
        {
            _logger.LogError(e, "{ReadWorkerName} Error reading job data from cache. REASON: Invalid format",
                GetType().Name);
            return Result<ExecutionQueueJobData, ErrorObject<string>>.Err(
                ErrorObject<string>.BadRequest("Error reading job data from cache. REASON: Invalid format"));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "{ReadWorkerName} Error reading job data from cache. REASON: -||-", GetType().Name);
            return Result<ExecutionQueueJobData, ErrorObject<string>>.Err(
                ErrorObject<string>.InternalError("Error reading job data from cache. REASON: -||-"));
        }
    }

    private static ExecutionDataDto BuildExecutionData(ExecutionQueueJobData jobData,
        SubmitExecuteResponse executionResponse, bool allTestsPassed)
    {
        return new ExecutionDataDto
        {
            ProblemId = jobData.ProblemId,
            UserId = jobData.UserId,
            Result = allTestsPassed
                ? jobData.JobType == JobType.Testing ? TestCaseResult.Accepted : TestCaseResult.Rejected
                : TestCaseResult.NotApplicable,
            Status = executionResponse.Status,
            ExitCode = executionResponse.ExecutionExitCode,
            ExecutionEndNs = executionResponse.ExecutionEndTimeNs,
            ExecutionStartNs = executionResponse.ExecutionStartTimeNs,
            JvmMemPeakKb = executionResponse.JvmMemoryPeakKb,
            ExecutionType = jobData.JobType,
            TestingResults = executionResponse.TestResults
        };
    }

    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
    }
}