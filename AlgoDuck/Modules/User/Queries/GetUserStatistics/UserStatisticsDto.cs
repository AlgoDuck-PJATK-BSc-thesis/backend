namespace AlgoDuck.Modules.User.Queries.GetUserStatistics;

public sealed class UserStatisticsDto
{
    public int TotalSolvedProblems { get; init; }
    public int TotalAttemptedProblems { get; init; }
    public int TotalSubmissions { get; init; }
    public int AcceptedSubmissions { get; init; }
    public int WrongAnswerSubmissions { get; init; }
    public int TimeLimitSubmissions { get; init; }
    public int RuntimeErrorSubmissions { get; init; }
    public double AcceptanceRate { get; init; }
    public double AverageAttemptsPerSolved { get; init; }
}