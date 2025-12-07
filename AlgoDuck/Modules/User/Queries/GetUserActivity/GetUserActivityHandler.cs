using AlgoDuck.Modules.User.Shared.Interfaces;

namespace AlgoDuck.Modules.User.Queries.GetUserActivity;

public sealed class GetUserActivityHandler : IGetUserActivityHandler
{
    private readonly IUserRepository _userRepository;

    public GetUserActivityHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserActivityDto>> HandleAsync(Guid userId, GetUserActivityRequestDto requestDto, CancellationToken cancellationToken)
    {
        if (requestDto.Page < 1)
        {
            requestDto = new GetUserActivityRequestDto
            {
                Page = 1,
                PageSize = requestDto.PageSize
            };
        }

        if (requestDto.PageSize < 1)
        {
            requestDto = new GetUserActivityRequestDto
            {
                Page = requestDto.Page,
                PageSize = 20
            };
        }

        var skip = (requestDto.Page - 1) * requestDto.PageSize;
        var solutions = await _userRepository.GetUserSolutionsAsync(userId, skip, requestDto.PageSize, cancellationToken);

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