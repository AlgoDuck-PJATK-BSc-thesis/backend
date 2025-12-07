using AlgoDuck.DAL;
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
        var userSolutions = _queryDbContext.UserSolutions
            .Include(s => s.Status)
            .Where(s => s.UserId == userId);

        var totalSubmissions = await userSolutions.CountAsync(cancellationToken);

        var acceptedSubmissions = await userSolutions
            .CountAsync(
                s => s.Status.StatusName == StatisticsConstants.StatusAccepted,
                cancellationToken
            );

        var wrongAnswerSubmissions = await userSolutions
            .CountAsync(
                s => s.Status.StatusName == StatisticsConstants.StatusWrongAnswer,
                cancellationToken
            );

        var timeLimitSubmissions = await userSolutions
            .CountAsync(
                s => s.Status.StatusName == StatisticsConstants.StatusTimeLimit,
                cancellationToken
            );

        var runtimeErrorSubmissions = await userSolutions
            .CountAsync(
                s => s.Status.StatusName == StatisticsConstants.StatusRuntimeError,
                cancellationToken
            );

        var totalSolved = await userSolutions
            .Where(s => s.Status.StatusName == StatisticsConstants.StatusAccepted)
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
        var query = _queryDbContext.UserSolutions
            .Include(s => s.Status)
            .Where(
                s =>
                    s.UserId == userId
                    && s.Status.StatusName == StatisticsConstants.StatusAccepted
            )
            .Select(s => s.ProblemId)
            .Distinct()
            .OrderBy(id => id);

        var problemIds = await query
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