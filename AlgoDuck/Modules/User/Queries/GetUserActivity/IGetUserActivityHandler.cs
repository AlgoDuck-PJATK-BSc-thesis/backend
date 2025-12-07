using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AlgoDuck.Modules.User.Queries.GetUserActivity;

public interface IGetUserActivityHandler
{
    Task<IReadOnlyList<UserActivityDto>> HandleAsync(Guid userId, GetUserActivityRequestDto requestDto, CancellationToken cancellationToken);
}