using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;

namespace AlgoDuck.Modules.Problem.Shared;

public interface IExecutionStatisticsRepository
{
    public Task<Result<bool, ErrorObject<string>>> RecordExecutionDataAsync(ExecutionDataDto executionData, CancellationToken cancellationToken = default);
}

public class ExecutionStatisticsRepository(
    ApplicationCommandDbContext applicationCommandDbContext
    ) : IExecutionStatisticsRepository
{
    public async Task<Result<bool, ErrorObject<string>>> RecordExecutionDataAsync(ExecutionDataDto executionData, CancellationToken cancellationToken = default)
    {
        var status = MapExecutorResponseToRdbStatus(executionData.Status);
        if (status.IsErr)
            return Result<bool, ErrorObject<string>>.Err(status.AsT1);
        try
        {
            await applicationCommandDbContext.CodeExecutionStatisticss.AddAsync(new CodeExecutionStatistics
            {
                UserId = executionData.UserId,
                ProblemId = executionData.ProblemId,
                Result = status.AsT0,
                TestCaseResult = executionData.Result
            }, cancellationToken);
            await applicationCommandDbContext.SaveChangesAsync(cancellationToken);
        }
        catch(Exception e)
        {
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.InternalError(e.Message));
        }
        return Result<bool, ErrorObject<string>>.Ok(true);
    }

    private Result<ExecutionResult, ErrorObject<string>> MapExecutorResponseToRdbStatus(
        SubmitExecuteRequestRabbitStatus status)
    {
        return status switch
        {
            SubmitExecuteRequestRabbitStatus.Queued or SubmitExecuteRequestRabbitStatus.Compiling
                or SubmitExecuteRequestRabbitStatus.Executing => Result<ExecutionResult, ErrorObject<string>>.Err(
                    ErrorObject<string>.BadRequest("cannot record intermediate execution status")),
            SubmitExecuteRequestRabbitStatus.Completed => Result<ExecutionResult, ErrorObject<string>>.Ok(
                ExecutionResult.Completed),
            SubmitExecuteRequestRabbitStatus.CompilationFailure => Result<ExecutionResult, ErrorObject<string>>.Ok(
                ExecutionResult.CompilationError),
            SubmitExecuteRequestRabbitStatus.RuntimeError => Result<ExecutionResult, ErrorObject<string>>.Ok(
                ExecutionResult.RuntimeError),
            SubmitExecuteRequestRabbitStatus.ServiceFailure => Result<ExecutionResult, ErrorObject<string>>.Ok(
                ExecutionResult.Completed),
            SubmitExecuteRequestRabbitStatus.Timeout => Result<ExecutionResult, ErrorObject<string>>.Ok(ExecutionResult
                .Timeout),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}

public class ExecutionDataDto
{
    public required Guid UserId { get; set; }
    public required Guid? ProblemId { get; set; }
    public required SubmitExecuteRequestRabbitStatus Status { get; set; }
    public required TestCaseResult Result { get; set; }
    
}