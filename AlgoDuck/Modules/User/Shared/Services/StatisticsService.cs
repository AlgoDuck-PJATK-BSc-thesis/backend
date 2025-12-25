using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Constants;
using AlgoDuck.Modules.User.Shared.DTOs;
using AlgoDuck.Modules.User.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Utils;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Shared.Services;

public sealed class StatisticsService : IStatisticsService
{
    private readonly ApplicationQueryDbContext _queryDbContext;

    public StatisticsService(ApplicationQueryDbContext queryDbContext)
    {
        _queryDbContext = queryDbContext;
    }

    public async Task<StatisticsSummary> GetStatisticsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userSolutions = _queryDbContext.CodeExecutionStatisticss
            .Where(s => s.UserId == userId);

        var totalSubmissions = await userSolutions.CountAsync(cancellationToken);

        var acceptedSubmissions = await userSolutions
            .CountAsync(
                s => s.TestCaseResult == TestCaseResult.Accepted,
                cancellationToken
            );

        var wrongAnswerSubmissions = await userSolutions
            .CountAsync(
                s => s.TestCaseResult == TestCaseResult.Rejected,
                cancellationToken
            );

        var timeLimitSubmissions = await userSolutions
            .CountAsync(
                s => s.Result == ExecutionResult.Timeout,
                cancellationToken
            );

        var runtimeErrorSubmissions = await userSolutions
            .CountAsync(
                s => s.Result == ExecutionResult.RuntimeError,
                cancellationToken
            );

        var totalSolved = await userSolutions
            .Where(s => s.TestCaseResult == TestCaseResult.Accepted)
            .Select(s => s.ProblemId)
            .Distinct()
            .CountAsync(cancellationToken);

        var totalAttempted = await userSolutions
            .Select(s => s.ProblemId)
            .Distinct()
            .CountAsync(cancellationToken);

        return StatisticsCalculator.Calculate(
            totalSolved,
            totalAttempted,
            totalSubmissions,
            acceptedSubmissions,
            wrongAnswerSubmissions,
            timeLimitSubmissions,
            runtimeErrorSubmissions
        );

    }

    public async Task<IReadOnlyList<SolvedProblemSummary>> GetSolvedProblemsAsync(
        Guid userId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var query = _queryDbContext.CodeExecutionStatisticss
            .Where(
                s =>
                    s.UserId == userId
                    && s.ProblemId != null /* since we track all executions we must allow a scenario where code is executed for no problem (perhaps in the algo visualizer?) hence the nullability and null check */
                    && s.TestCaseResult == TestCaseResult.Accepted
            )
            .Select(s => (Guid) s.ProblemId!)
            .Distinct()
            .OrderBy(id => id);

        var problemIds = await query
            .Select(id => id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return problemIds
            .Select(id => new SolvedProblemSummary
            {
                ProblemId = id
            })
            .ToList();

    }
}