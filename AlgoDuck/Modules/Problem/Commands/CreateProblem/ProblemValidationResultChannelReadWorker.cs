using System.Text;
using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Shared;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using StackExchange.Redis;

namespace AlgoDuck.Modules.Problem.Commands.CreateProblem;

public sealed class ProblemValidationResultChannelReadWorker(
    IRabbitMqConnectionService rabbitMqConnectionService,
    IHubContext<CreateProblemUpdatesHub> hubContext,
    IDatabase redis,
    IServiceScopeFactory scopeFactory
) : BackgroundService, IAsyncDisposable
{
    private IChannel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await rabbitMqConnectionService.GetConnection();

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
                Status = NarrowDownExecutorResponseStatus(response.Status)
            };

            var jobDataResult = await GetJobDataAsync(response.JobId);

            if (jobDataResult.IsErr)
                return;
            var jobData = jobDataResult.AsT0;

            jobData.CachedResponses.Add(validationResponse);
            
            await redis.StringSetAsync(
                key: new RedisKey(response.JobId.ToString()),
                value: JsonSerializer.Serialize(jobData),
                expiry: TimeSpan.FromMinutes(3));
            await hubContext.Clients.Group(response.JobId.ToString()).SendAsync(
                method: "ValidationStatusUpdated",
                arg1: validationResponse,
                cancellationToken: cancellationToken);
            
            /* ReSharper disable once SwitchStatementMissingSomeEnumCasesNoDefault - other cases intentionally falling through*/
            switch (validationResponse.Status)
            {
                case ValidationResponseStatus.Succeeded:
                    await HandleSuccessAsync(jobData.ProblemId, cancellationToken);
                    break;
                case ValidationResponseStatus.Failed:
                    await HandleFailureAsync(jobData.ProblemId, cancellationToken);
                    break;
            }
        }

        await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken);
    }

    /*TODO: make these service s3 as well as the rdb*/
    private async Task HandleSuccessAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationCommandDbContext>();
        
        await dbContext.Problems
            .Where(p => p.ProblemId == problemId)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(p => p.Status, ProblemStatus.Verified), cancellationToken: cancellationToken);
        
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task HandleFailureAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        using var scope = scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationCommandDbContext>();
        
        await dbContext.TestCases
            .Where(t => t.ProblemProblemId == problemId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);

        await dbContext.Problems
            .Where(p => p.ProblemId == problemId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);


        await dbContext.SaveChangesAsync(cancellationToken);
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

    private async Task<Result<JobData, string>> GetJobDataAsync(Guid jobId)
    {
        var jobDataRaw = await redis.StringGetAsync(jobId.ToString());
        if (jobDataRaw.IsNullOrEmpty)
            return Result<JobData, string>.Err("could not get job data");
        try
        {
            var jobData = JsonSerializer.Deserialize<JobData>(jobDataRaw.ToString());
            return jobData == null
                ? Result<JobData, string>.Err("could not get jobId")
                : Result<JobData, string>.Ok(jobData);
        }
        catch(Exception)
        {
            return Result<JobData, string>.Err("could not get jobId");
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
            SubmitExecuteRequestRabbitStatus.Failed => ValidationResponseStatus.Failed,
            SubmitExecuteRequestRabbitStatus.TimedOut => ValidationResponseStatus.Failed,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_channel != null) await _channel.DisposeAsync();
    }
}