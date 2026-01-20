using System.Text.Json;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Queries.AdminGetProblemStats;
using AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin;
using AlgoDuck.Modules.Problem.Shared.Types;
using AlgoDuck.Shared.Http;
using AlgoDuckShared;

namespace AlgoDuck.Modules.Problem.Shared.Repositories;

public interface IExecutionStatisticsRepository
{
    public Task<Result<bool, ErrorObject<string>>> RecordExecutionDataAsync(ExecutionDataDto executionData, CancellationToken cancellationToken = default);
}

public class ExecutionStatisticsRepository : IExecutionStatisticsRepository
{
    private readonly ApplicationCommandDbContext _applicationCommandDbContext;

    public ExecutionStatisticsRepository(ApplicationCommandDbContext applicationCommandDbContext)
    {
        _applicationCommandDbContext = applicationCommandDbContext;
    }

    public async Task<Result<bool, ErrorObject<string>>> RecordExecutionDataAsync(ExecutionDataDto executionData, CancellationToken cancellationToken = default)
    {
        var status = MapExecutorResponseToRdbStatus(executionData.Status);
        if (status.IsErr)
            return Result<bool, ErrorObject<string>>.Err(status.AsT1);
        try
        {
            var codeExecutionId = Guid.NewGuid();
            var timeStampNowNanos = DateTime.Now.DateTimeToNanos();
            await _applicationCommandDbContext.CodeExecutionStatisticss.AddAsync(new CodeExecutionStatistics
            {
                CodeExecutionId = codeExecutionId,
                UserId = executionData.UserId,
                ProblemId = executionData.ProblemId,
                Result = status.AsT0,
                TestCaseResult = executionData.Result,
                ExecutionType = executionData.ExecutionType,
                ExecutionStartNs = executionData.ExecutionStartNs == 0 ? timeStampNowNanos : executionData.ExecutionStartNs,
                ExecutionEndNs = executionData.ExecutionStartNs  == 0 ? timeStampNowNanos : executionData.ExecutionEndNs, // intentional repetition, these can only diverge with 1 being 0 and 2 not if something breaks. This is defensive
                JvmPeakMemKb = executionData.JvmMemPeakKb,
                ExitCode = executionData.ExitCode,
                TestingResults = executionData.TestingResults.Select(t => new TestingResult
                {
                    IsPassed = t.IsTestPassed,
                    CodeExecutionId = codeExecutionId,
                    TestCaseId = t.TestId
                }).ToList()
            }, cancellationToken);
            await _applicationCommandDbContext.SaveChangesAsync(cancellationToken);
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
    public required JobType ExecutionType { get; set; }
    public required long ExecutionStartNs { get; set; }
    public required long ExecutionEndNs { get; set; }
    public required long JvmMemPeakKb { get; set; }
    public required int ExitCode { get; set; }
    public ICollection<TestResultDto> TestingResults { get; set; } = [];
}
