using AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin;


public interface IProblemDetailsAdminService
{
    public Task<Result<ProblemStatsDto, ErrorObject<string>>> GetProblemStatsAsync(PerformanceRequest performanceRequest, RecentActivityRequest recentActivityRequest, CancellationToken cancellationToken = default);


}

public class ProblemDetailsAdminService : IProblemDetailsAdminService
{
    private readonly IProblemDetailsAdminRepository _repository;

    public ProblemDetailsAdminService(IProblemDetailsAdminRepository repository)
    {
        _repository = repository;
    }


    public async Task<Result<ProblemStatsDto, ErrorObject<string>>> GetProblemStatsAsync(PerformanceRequest performanceRequest, RecentActivityRequest recentActivityRequest,
        CancellationToken cancellationToken = default)
    {
        var problemStats = new ProblemStatsDto();

        return await _repository.GetAttemptMetricsAsync(performanceRequest.ProblemId, cancellationToken)
            .BindAsync(async attemptMetrics =>
            {
                problemStats.AttemptMetrics = attemptMetrics;
                return await _repository.GetChurnMetricsAsync(performanceRequest.ProblemId, cancellationToken);
            }).BindAsync(async churnMetrics =>
            {
                problemStats.ChurnMetrics = churnMetrics;
                return await _repository.GetPerformanceMetricsAsync(performanceRequest, cancellationToken);
            }).BindAsync(async performanceMetrics =>
            {
                problemStats.PerformanceMetrics = performanceMetrics;
                return await _repository.GetProblemDetailsCoreAsync(performanceRequest.ProblemId, cancellationToken);
            }).BindAsync(async problemDetailsCore =>
            {
                problemStats.ProblemDetailsCore = problemDetailsCore;
                return await _repository.GetRecentSubmissionsAsync(recentActivityRequest, cancellationToken);
            }).BindAsync(async recentSubmissions =>
            {
                problemStats.RecentSubmission = recentSubmissions;
                return await _repository.GetTestCaseStatsAsync(performanceRequest.ProblemId, cancellationToken);
            }).BindAsync(async testCaseStats =>
            {
                problemStats.TestCaseStats = testCaseStats;
                return await _repository.GetTimeSeriesMetricsAsync(new TimeSeriesRequest
                {
                    ProblemId = performanceRequest.ProblemId,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow,
                    Granularity = TimeSeriesGranularity.Day
                }, cancellationToken);
            })
            .BindResult<TimeSeriesMetrics, ProblemStatsDto, ErrorObject<string>>(timeSeriesMetrics =>
            {
                problemStats.TimeSeriesMetrics = timeSeriesMetrics;
                return Result<ProblemStatsDto, ErrorObject<string>>.Ok(problemStats);
            });
    }
}