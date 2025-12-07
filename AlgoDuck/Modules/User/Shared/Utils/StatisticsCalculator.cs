using AlgoDuck.Modules.User.Shared.DTOs;

namespace AlgoDuck.Modules.User.Shared.Utils;

public static class StatisticsCalculator
{
    public static StatisticsSummary Calculate(
        int totalSolved,
        int totalAttempted,
        int totalSubmissions,
        int acceptedSubmissions,
        int wrongAnswerSubmissions,
        int timeLimitSubmissions,
        int runtimeErrorSubmissions)
    {
        var acceptanceRate = totalSubmissions == 0
            ? 0.0
            : (double)acceptedSubmissions / totalSubmissions;

        var averageAttemptsPerSolved = totalSolved == 0
            ? 0.0
            : (double)totalAttempted / totalSolved;

        return new StatisticsSummary
        {
            TotalSolvedProblems = totalSolved,
            TotalAttemptedProblems = totalAttempted,
            TotalSubmissions = totalSubmissions,
            AcceptedSubmissions = acceptedSubmissions,
            WrongAnswerSubmissions = wrongAnswerSubmissions,
            TimeLimitSubmissions = timeLimitSubmissions,
            RuntimeErrorSubmissions = runtimeErrorSubmissions,
            AcceptanceRate = acceptanceRate,
            AverageAttemptsPerSolved = averageAttemptsPerSolved
        };
    }
}