using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Problem.Commands.CodeExecuteSubmission;
using AlgoDuck.Modules.Problem.Queries.AdminGetProblemStats.Types;
using AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetProblemStats;

public interface IProblemDetailsAdminRepository
{
    public Task<Result<ProblemDetailsCore, ErrorObject<string>>> GetProblemDetailsCoreAsync(Guid problemId,
        CancellationToken cancellationToken = default);

    public Task<Result<ChurnMetrics, ErrorObject<string>>> GetChurnMetricsAsync(Guid problemId,
        CancellationToken cancellationToken = default);

    public Task<Result<PerformanceMetrics, ErrorObject<string>>> GetPerformanceMetricsAsync(
        PerformanceRequest performanceRequest, CancellationToken cancellationToken = default);

    public Task<Result<ICollection<RecentSubmissionDto>, ErrorObject<string>>> GetRecentSubmissionsAsync(
        RecentActivityRequest recentActivityRequest, CancellationToken cancellationToken = default);

    public Task<Result<ICollection<TestCaseStats>, ErrorObject<string>>> GetTestCaseStatsAsync(Guid problemId,
        CancellationToken cancellationToken = default);

    public Task<Result<AttemptMetrics, ErrorObject<string>>> GetAttemptMetricsAsync(Guid problemId,
        CancellationToken cancellationToken = default);

    public Task<Result<TimeSeriesMetrics, ErrorObject<string>>> GetTimeSeriesMetricsAsync(
        TimeSeriesRequest request,
        CancellationToken cancellationToken = default);
}

public class ProblemDetailsAdminRepository : IProblemDetailsAdminRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public ProblemDetailsAdminRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ProblemDetailsCore, ErrorObject<string>>> GetProblemDetailsCoreAsync(Guid problemId,
        CancellationToken cancellationToken = default)
    {
        var firstOrDefault = await _dbContext.Problems.Include(p => p.Difficulty).Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.ProblemId == problemId, cancellationToken: cancellationToken);

        if (firstOrDefault is null)
            return Result<ProblemDetailsCore, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound($"Problem {problemId} not found"));

        return Result<ProblemDetailsCore, ErrorObject<string>>.Ok(new ProblemDetailsCore
        {
            Category = new CategoryDto
            {
                Name = firstOrDefault.Category.CategoryName,
                Id = firstOrDefault.Category.CategoryId
            },
            CreatedAt = firstOrDefault.CreatedAt,
            CreatedBy = firstOrDefault.CreatedByUserId,
            Difficulty = new DifficultyDto
            {
                Id = firstOrDefault.DifficultyId,
                Name = firstOrDefault.Difficulty.DifficultyName,
                RewardScaler = firstOrDefault.Difficulty.RewardScaler
            },
            LastUpdatedAt = firstOrDefault.LastUpdatedAt,
            ProblemId = problemId,
            ProblemName = firstOrDefault.ProblemTitle
        });
    }

    public async Task<Result<ChurnMetrics, ErrorObject<string>>> GetChurnMetricsAsync(Guid problemId,
        CancellationToken cancellationToken = default)
    {
        var snapshotUserIds = await _dbContext.UserSolutionSnapshots
            .Where(p => p.ProblemId == problemId)
            .Select(u => u.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var statisticsUserIds = await _dbContext.CodeExecutionStatisticss
            .Where(p => p.ProblemId == problemId && p.ExecutionType == JobType.Testing)
            .Select(u => u.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);

        var allInteractedUserIds = snapshotUserIds
            .Union(statisticsUserIds)
            .Distinct()
            .Count();

        return Result<ChurnMetrics, ErrorObject<string>>.Ok(new ChurnMetrics
        {
            UserStartCount = allInteractedUserIds,
            UserFinishCount = await _dbContext.CodeExecutionStatisticss
                .Where(p => p.ProblemId == problemId && p.TestCaseResult == TestCaseResult.Accepted)
                .Select(u => u.UserId)
                .Distinct()
                .CountAsync(cancellationToken),
            UserSubmitCount = statisticsUserIds.Count,
        });
    }

    public async Task<Result<PerformanceMetrics, ErrorObject<string>>> GetPerformanceMetricsAsync(
        PerformanceRequest performanceRequest, CancellationToken cancellationToken = default)
    {
        var allRuntimeRangesForProblem = await _dbContext.CodeExecutionStatisticss.Where(s => s.Result == ExecutionResult.Completed)
            .Where(p => p.ProblemId == performanceRequest.ProblemId).Select(p =>
                new { p.ExecutionStartNs, p.ExecutionEndNs, p.JvmPeakMemKb })
            .ToListAsync(cancellationToken: cancellationToken);

        if (allRuntimeRangesForProblem.Count == 0)
        {
            return Result<PerformanceMetrics, ErrorObject<string>>.Ok(new PerformanceMetrics
            {
                AverageRuntimeNs = 0,
                MedianRuntimeMs = 0,
                P95RuntimeMs = 0,
                AvgJvmMemoryUsageKb = 0,
                MemoryBuckets = [],
                RuntimeBuckets = [],
            });
        }

        var allRuntimesForProblem =
            allRuntimeRangesForProblem.Select(r => r.ExecutionEndNs - r.ExecutionStartNs).ToList();

        allRuntimesForProblem.Sort();

        var problemRuntimeAvg = (long)allRuntimesForProblem.Average();

        var problemRuntimeMed = allRuntimesForProblem.Count == 1
            ? allRuntimesForProblem[0]
            : allRuntimesForProblem.Count % 2 == 1
                ? allRuntimesForProblem[allRuntimesForProblem.Count / 2]
                : (allRuntimesForProblem[allRuntimesForProblem.Count / 2 - 1] +
                   allRuntimesForProblem[allRuntimesForProblem.Count / 2]) / 2;

        var p95Index = (int)Math.Floor(allRuntimesForProblem.Count * 0.05);
        var problemRuntimeP95 = allRuntimesForProblem[Math.Min(p95Index, allRuntimesForProblem.Count - 1)];

        var allMemoryUsage = allRuntimeRangesForProblem.Select(p => p.JvmPeakMemKb).ToList();
        var memoryUsageAvg = (long)allMemoryUsage.Average();
        allMemoryUsage.Sort();

        var minRuntime = allRuntimesForProblem[0];
        var maxRuntime = allRuntimesForProblem[^1];
        var runtimeBucketCount = (int)((maxRuntime - minRuntime) / performanceRequest.RuntimeBucketSize) + 1;

        var runtimeCounts = allRuntimesForProblem
            .GroupBy(p => (p - minRuntime) / performanceRequest.RuntimeBucketSize)
            .ToDictionary(g => g.Key, g => g.Count());

        var runtimeBuckets = Enumerable.Range(0, runtimeBucketCount)
            .Select(i => new BucketData
            {
                Range = $"{NanosToMillis((long)i * performanceRequest.RuntimeBucketSize + minRuntime)} - {NanosToMillis(((long)i + 1) * performanceRequest.RuntimeBucketSize + minRuntime)}",
                Count = runtimeCounts.GetValueOrDefault(i, 0)
            }).ToList();

        var minMemory = allMemoryUsage[0];
        var maxMemory = allMemoryUsage[^1];
        var memoryBucketCount = (int)((maxMemory - minMemory) / performanceRequest.MemoryBucketSize) + 1;

        var memoryCounts = allMemoryUsage
            .GroupBy(p => (p - minMemory) / performanceRequest.MemoryBucketSize)
            .ToDictionary(g => g.Key, g => g.Count());

        var memoryUsageBuckets = Enumerable.Range(0, memoryBucketCount)
            .Select(i => new BucketData
            {
                Range = $"{(long)i * performanceRequest.MemoryBucketSize + minMemory} - {((long)i + 1) * performanceRequest.MemoryBucketSize + minMemory}KB",
                Count = memoryCounts.GetValueOrDefault(i, 0)
            }).ToList();

        return Result<PerformanceMetrics, ErrorObject<string>>.Ok(new PerformanceMetrics
        {
            AverageRuntimeNs = problemRuntimeAvg,
            MedianRuntimeMs = problemRuntimeMed,
            P95RuntimeMs = problemRuntimeP95,
            AvgJvmMemoryUsageKb = memoryUsageAvg,
            MemoryBuckets = memoryUsageBuckets,
            RuntimeBuckets = runtimeBuckets,
        });
    }

    public async Task<Result<ICollection<RecentSubmissionDto>, ErrorObject<string>>> GetRecentSubmissionsAsync(
        RecentActivityRequest recentActivityRequest,
        CancellationToken cancellationToken = default)
    {
        var recentSubmissionsWithLongTimestamp = await _dbContext
            .CodeExecutionStatisticss
            .Include(s => s.ApplicationUser)
            .Where(e => e.ExecutionType == JobType.Testing && e.ProblemId == recentActivityRequest.ProblemId)
            .OrderByDescending(q => q.ExecutionStartNs)
            .Take(recentActivityRequest.RecentCount).Select(a => new
                {
                    a.UserId,
                    Username = a.ApplicationUser.UserName ?? "<undefined>", /* coalescing should not happen */
                    RuntimeNs = a.ExecutionEndNs - a.ExecutionStartNs,
                    Status = a.Result,
                    SubmittedAt = a.ExecutionStartNs,
                    SubmissionId = a.CodeExecutionId,
                }
            ).ToListAsync(cancellationToken);
        
        return Result<ICollection<RecentSubmissionDto>, ErrorObject<string>>.Ok(recentSubmissionsWithLongTimestamp
            .Select(s => new RecentSubmissionDto
            {
                UserId = s.UserId,
                Username = s.Username,
                RuntimeNs = s.RuntimeNs,
                Status = s.Status,
                SubmittedAt = s.SubmittedAt.LongToDateTime(),
                SubmissionId = s.SubmissionId
            }).ToList());
    }

    public async Task<Result<ICollection<TestCaseStats>, ErrorObject<string>>> GetTestCaseStatsAsync(Guid problemId,
        CancellationToken cancellationToken = default)
    {
        return Result<ICollection<TestCaseStats>, ErrorObject<string>>.Ok(await _dbContext.TestCases
            .Include(p => p.TestingResults)
            .Where(p => problemId == p.ProblemProblemId).Select(t => new TestCaseStats
            {
                TestCaseId = t.TestCaseId,
                IsPublic = t.IsPublic,
                PassRate = t.TestingResults.Select(tr => tr.IsPassed ? 1 : 0).DefaultIfEmpty().Average()
            }).ToListAsync(cancellationToken: cancellationToken));
    }

    public async Task<Result<AttemptMetrics, ErrorObject<string>>> GetAttemptMetricsAsync(Guid problemId,
        CancellationToken cancellationToken = default)
    {
        var attemptMetrics = await _dbContext.CodeExecutionStatisticss
            .Where(e => e.ProblemId == problemId)
            .Select(p => new AttemptMetricsDao
            {
                UserId = p.UserId,
                IsAccepted = p.TestCaseResult == TestCaseResult.Accepted
            }).ToListAsync(cancellationToken: cancellationToken);

        return Result<AttemptMetrics, ErrorObject<string>>.Ok(new AttemptMetrics
        {
            AcceptanceRate = attemptMetrics.Select(a => a.IsAccepted ? 1 : 0).DefaultIfEmpty().Average(),
            AcceptedAttempts = attemptMetrics.Count(a => a.IsAccepted),
            AcceptedUniqueAttempts = attemptMetrics.Where(a => a.IsAccepted).Select(a => a.UserId).Distinct().Count(),
            TotalAttempts = attemptMetrics.Count,
            UniqueAttempts = attemptMetrics.Select(a => a.UserId).Distinct().Count(),
        });
    }

    public async Task<Result<TimeSeriesMetrics, ErrorObject<string>>> GetTimeSeriesMetricsAsync(
        TimeSeriesRequest request,
        CancellationToken cancellationToken = default)
    {
        var lower = request.StartDate.DateTimeToNanos();
        var upper = request.EndDate.DateTimeToNanos();
        var submissions = await _dbContext.CodeExecutionStatisticss
            .Where(e => e.ProblemId == request.ProblemId && e.ExecutionType == JobType.Testing)
            .Select(e => new
            {
                Timestamp = e.ExecutionStartNs,
                IsPassed = e.TestCaseResult == TestCaseResult.Accepted
            })
            .Where(e => e.Timestamp >= lower && e.Timestamp <= upper)
            .ToListAsync(cancellationToken);

        var grouped = submissions
            .GroupBy(s => request.Granularity switch
            {
                TimeSeriesGranularity.Hour => s.Timestamp.LongToDateTime().ToString("yyyy-MM-dd HH:00"),
                TimeSeriesGranularity.Day => s.Timestamp.LongToDateTime().ToString("yyyy-MM-dd"),
                TimeSeriesGranularity.Week => GetWeekStart(s.Timestamp.LongToDateTime()).ToString("yyyy-MM-dd"),
                TimeSeriesGranularity.Month => s.Timestamp.LongToDateTime().ToString("yyyy-MM"),
                _ => s.Timestamp.LongToDateTime().ToString("yyyy-MM-dd")
            })
            .OrderBy(g => g.Key)
            .ToList();

        var acceptanceHistory = grouped.Select(g => new AcceptanceRatePoint
        {
            Date = g.Key,
            Rate = g.Any() ? g.Count(s => s.IsPassed) * 100.0 / g.Count() : 0
        }).ToList();

        var submissionsOverTime = grouped.Select(g => new SubmissionPoint
        {
            Date = g.Key,
            Count = g.Count(),
            Passed = g.Count(s => s.IsPassed)
        }).ToList();

        return Result<TimeSeriesMetrics, ErrorObject<string>>.Ok(new TimeSeriesMetrics
        {
            AcceptanceRateHistory = acceptanceHistory,
            SubmissionsOverTime = submissionsOverTime
        });
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private static long NanosToMillis(long nanos)
    {
        return nanos / 1_000_000L;
    }
}

public static class DateTimeExtensions
{
    public static long DateTimeToNanos(this DateTime dt)
    {
        var unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (dt.ToUniversalTime() - unixEpoch).Ticks * 100;
    }

    public static DateTime LongToDateTime(this long l)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddTicks(l / 100);
    }
}