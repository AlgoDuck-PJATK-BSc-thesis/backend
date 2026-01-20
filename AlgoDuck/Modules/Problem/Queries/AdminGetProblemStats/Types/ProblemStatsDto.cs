using AlgoDuck.Modules.Problem.Queries.AdminGetProblemStats.Types;

namespace AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;

public class ProblemStatsDto
{
    public AttemptMetrics AttemptMetrics { get; set; } = null!;
    public ProblemDetailsCore ProblemDetailsCore { get; set; } = null!;
    public PerformanceMetrics PerformanceMetrics { get; set; } = null!;
    public ChurnMetrics ChurnMetrics { get; set; } = null!;
    public ICollection<RecentSubmissionDto> RecentSubmission { get; set; } = null!;
    public ICollection<TestCaseStats> TestCaseStats { get; set; } = null!;
    public TimeSeriesMetrics TimeSeriesMetrics { get; set; } = null!;
}