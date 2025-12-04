namespace AlgoDuck.Modules.User.Queries.GetUserStatistics;

public interface IGetUserStatisticsHandler
{
    Task<UserStatisticsDto> HandleAsync(Guid userId, CancellationToken cancellationToken);
}