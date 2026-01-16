using System.Text;
using System.Text.Json;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.CreateProblem;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpdateProblem;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Extensions;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;
using Microsoft.AspNetCore.SignalR;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertUtils;

public sealed class ProblemValidationResultChannelReadWorker(
    IRabbitMqConnectionService rabbitMqConnectionService,
    IHubContext<CreateProblemUpdatesHub> hubContext,
    IDatabase redis,
    IServiceScopeFactory scopeFactory
) : BackgroundService, IAsyncDisposable
{
    private IChannel? _channel;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions().WithInternalFields();
    

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await rabbitMqConnectionService.GetConnection(stoppingToken);

        _channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

        await _channel.QueueDeclareAsync(queue: "problem_validation_results",
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += (sender, ea) => HandleMessageAsync(ea, stoppingToken);

        await _channel.BasicConsumeAsync(
            queue: "problem_validation_results",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken);

        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken cancellationToken = default)
    {
        if (_channel == null) return;

        var result = GetResponseObject<ExecutionResponseRabbit>(ea);
        if (result.IsErr)
        {
            await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true,
                cancellationToken: cancellationToken);
            return;
        }

        var response = result.AsT0;
        if (response != null)
        {
            var validationResponse = new ValidationResponse
            {
                Status = NarrowDownExecutorResponseStatus(response.Status),
                Message = response.Err /* The reason why we only bounce Err is the client here is only interested in potential failures, i.e. illegal statements in template causing compilation failures, not actual execution output */
            };

            var jobDataResult = await GetJobDataFromCacheAsync(response.JobId);

            if (jobDataResult.IsErr)
                return;
            var jobData = jobDataResult.AsT0;

            jobData.CachedResponses.Add(validationResponse);

            await redis.StringSetAsync(
                key: new RedisKey(response.JobId.ToString()),
                value: JsonSerializer.Serialize(jobData, _jsonSerializerOptions),
                expiry: TimeSpan.FromMinutes(3));
            
            await hubContext.Clients.Group(response.JobId.ToString()).SendAsync(
                method: "ValidationStatusUpdated",
                arg1: new StandardApiResponse<ValidationResponse>
                {
                    Body = validationResponse
                },
                cancellationToken: cancellationToken);
            
            /* ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault - other cases intentionally falling through*/
            switch (validationResponse.Status)
            {
                case ValidationResponseStatus.Succeeded:
                    await HandleSuccessAsync(jobData, cancellationToken);
                    break;
                case ValidationResponseStatus.Failed:
                    await HandleFailureAsync(jobData, cancellationToken);
                    break;
            }
        }

        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
    }

    /*TODO: make these service s3 as well as the rdb*/
    private async Task<Result<Guid, ErrorObject<string>>> HandleSuccessAsync(JobData<UpsertProblemDto> upsertProblemDto, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        
        switch (upsertProblemDto.UpsertJobType)
        {
            case UpsertJobType.Update:
                if (upsertProblemDto.JobBody == null)
                    return Result<Guid, ErrorObject<string>>.Err(ErrorObject<string>.InternalError($"Could not update problem {upsertProblemDto.ProblemId}"));
                var updateContext = scope.ServiceProvider.GetRequiredService<IUpdateProblemRepository>();
                return await updateContext.UpdateProblemAsync(upsertProblemDto.JobBody, upsertProblemDto.ProblemId, cancellationToken);
            case UpsertJobType.Insert:
                var insertContext = scope.ServiceProvider.GetRequiredService<ICreateProblemRepository>();
                return await insertContext.ConfirmProblemUponValidationSuccessAsync(upsertProblemDto.ProblemId, cancellationToken);
            default:
                throw new ArgumentOutOfRangeException();
        }

    }

    private async Task<Result<Guid, ErrorObject<string>>> HandleFailureAsync(JobData<UpsertProblemDto> upsertProblemDto, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ICreateProblemRepository>();

        return await dbContext.DeleteProblemUponValidationFailureAsync(upsertProblemDto.ProblemId, cancellationToken);
    }
    
    private static Result<T, string> GetResponseObject<T>(BasicDeliverEventArgs eventArgs)
    {
        try
        {
            
            var body = eventArgs.Body;
            var message = Encoding.UTF8.GetString(body.ToArray());
            var response = DefaultJsonSerializer.Deserialize<T>(message);
            return response == null ? Result<T, string>.Err("could not get response object") : Result<T, string>.Ok(response);
        }
        catch (Exception e)
        {
            return Result<T, string>.Err("could not get response object");
        }
    }

    private async Task<Result<JobData<UpsertProblemDto>, string>> GetJobDataFromCacheAsync(Guid jobId)
    {
        var jobDataRaw = await redis.StringGetAsync(jobId.ToString());
        if (jobDataRaw.IsNullOrEmpty)
            return Result<JobData<UpsertProblemDto>, string>.Err("could not get job data");
        try
        {
            var jobData = JsonSerializer.Deserialize<JobData<UpsertProblemDto>>(
                jobDataRaw.ToString(), 
                _jsonSerializerOptions);
            
            return jobData == null
                ? Result<JobData<UpsertProblemDto>, string>.Err("could not get jobId")
                : Result<JobData<UpsertProblemDto>, string>.Ok(jobData);
        }
        catch(Exception)
        {
            return Result<JobData<UpsertProblemDto>, string>.Err("could not get jobId");
        }
    }
    
    private ValidationResponseStatus NarrowDownExecutorResponseStatus(SubmitExecuteRequestRabbitStatus status)
    {
        return status switch
        {
            SubmitExecuteRequestRabbitStatus.Queued => ValidationResponseStatus.Queued,
            SubmitExecuteRequestRabbitStatus.Compiling => ValidationResponseStatus.Pending,
            SubmitExecuteRequestRabbitStatus.Executing => ValidationResponseStatus.Pending,
            SubmitExecuteRequestRabbitStatus.Completed => ValidationResponseStatus.Succeeded,
            SubmitExecuteRequestRabbitStatus.ServiceFailure => ValidationResponseStatus.Failed,            
            SubmitExecuteRequestRabbitStatus.CompilationFailure => ValidationResponseStatus.Failed,
            SubmitExecuteRequestRabbitStatus.RuntimeError => ValidationResponseStatus.Failed,
            SubmitExecuteRequestRabbitStatus.Timeout => ValidationResponseStatus.Failed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
    }
}