using AlgoDuck.DAL;
using AlgoDuck.Models;
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
        var allSubmissions = await _queryDbContext.CodeExecutionStatisticss
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);

        var totalSubmissions = allSubmissions.Count;

        var acceptedSubmissions = allSubmissions
            .Count(s => s.TestCaseResult == TestCaseResult.Accepted);

        var wrongAnswerSubmissions = allSubmissions
            .Count(s => s.TestCaseResult == TestCaseResult.Rejected);

        var timeLimitSubmissions = allSubmissions
            .Count(s => s.Result == ExecutionResult.Timeout);

        var runtimeErrorSubmissions = allSubmissions
            .Count(s => s.Result == ExecutionResult.RuntimeError);

        var totalSolved = allSubmissions
            .Where(s => s.TestCaseResult == TestCaseResult.Accepted && s.ProblemId.HasValue)
            .Select(s => s.ProblemId!.Value)
            .Distinct()
            .Count();

        var totalAttempted = allSubmissions
            .Where(s => s.ProblemId.HasValue)
            .Select(s => s.ProblemId!.Value)
            .Distinct()
            .Count();

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
        var problemIds = await _queryDbContext.CodeExecutionStatisticss
            .Where(s =>
                s.UserId == userId
                && s.ProblemId != null
                && s.TestCaseResult == TestCaseResult.Accepted
            )
            .Select(s => s.ProblemId!.Value)
            .Distinct()
            .OrderBy(id => id)
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
