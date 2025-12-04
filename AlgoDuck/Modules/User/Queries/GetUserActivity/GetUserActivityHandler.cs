using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetUserActivity;

public sealed class GetUserActivityHandler : IGetUserActivityHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserActivityHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserActivityDto>> HandleAsync(Guid userId, GetUserActivityQuery query, CancellationToken cancellationToken)
    {
        if (query.Page < 1)
        {
            query = new GetUserActivityQuery
            {
                Page = 1,
                PageSize = query.PageSize
            };
        }

        if (query.PageSize < 1)
        {
            query = new GetUserActivityQuery
            {
                Page = query.Page,
                PageSize = 20
            };
        }

        var skip = (query.Page - 1) * query.PageSize;
        var solutions = await _userRepository.GetUserSolutionsAsync(userId, skip, query.PageSize, cancellationToken);

        var result = solutions
            .Select(s => new UserActivityDto
            {
                SolutionId = s.SolutionId,
                ProblemId = s.ProblemId,
                ProblemName = s.Problem?.ProblemTitle ?? string.Empty,
                StatusName = s.Status?.StatusName ?? string.Empty,
                CodeRuntimeSubmitted = s.CodeRuntimeSubmitted,
                SubmittedAt = s.CreatedAt
            })
            .ToList();

        return result;
    }
}