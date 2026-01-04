namespace AlgoDuck.Modules.User.Queries.User.Stats.GetUserStatistics;

public interface IGetUserStatisticsHandler
{
    Task<UserStatisticsDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}