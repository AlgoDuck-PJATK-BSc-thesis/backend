namespace AlgoDuck.Modules.User.Queries.User.Activity.GetUserActivity;

public interface IGetUserActivityHandler
{
    Task<IReadOnlyList<UserActivityDto>> HandleAsync(Guid userId, GetUserActivityRequestDto requestDto, CancellationToken cancellationToken);
}