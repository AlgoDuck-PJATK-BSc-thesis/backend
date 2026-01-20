using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetUserSolutionsForProblem;
    
public interface IUserSolutionService
{
    public Task<Result<PageData<UserSolutionDto>, ErrorObject<string>>> GetAllUserSolutionsAsync(UserSolutionRequestDto userSolutionRequestDto, CancellationToken cancellationToken = default);
}

public class UserSolutionService : IUserSolutionService
{
    private readonly IUserSolutionRepository _userSolutionRepository;

    public UserSolutionService(IUserSolutionRepository userSolutionRepository)
    {
        _userSolutionRepository = userSolutionRepository;
    }

    public async Task<Result<PageData<UserSolutionDto>, ErrorObject<string>>> GetAllUserSolutionsAsync(UserSolutionRequestDto userSolutionRequestDto, CancellationToken cancellationToken = default)
    {
        return await _userSolutionRepository.GetAllUserSolutionsAsync(userSolutionRequestDto, cancellationToken);
    }
}