namespace AlgoDuck.Modules.User.Queries.User.Stats.GetUserStatistics;

public sealed class UserStatisticsDto
{
    public int TotalSolvedProblems { get; init; }
    public int TotalAttemptedProblems { get; init; }
    public int TotalSubmissions { get; init; }
    public int Accepted { get; init; }
    public int WrongAnswer { get; init; }
    public int TimeLimitExceeded { get; init; }
    public int RuntimeError { get; init; }
    public double AcceptanceRate { get; init; }
    public double AvgAttemptsPerSolved { get; init; }
}