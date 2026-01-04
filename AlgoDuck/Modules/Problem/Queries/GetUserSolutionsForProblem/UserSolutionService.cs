using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetUserSolutionsForProblem;
    
public interface IUserSolutionService
{
    public Task<Result<PageData<UserSolutionDto>, ErrorObject<string>>> GetAllUserSolutionsAsync(UserSolutionRequestDto userSolutionRequestDto, CancellationToken cancellationToken = default);
}

public class UserSolutionService(
    IUserSolutionRepository userSolutionRepository
    ) : IUserSolutionService
{
    public async Task<Result<PageData<UserSolutionDto>, ErrorObject<string>>> GetAllUserSolutionsAsync(UserSolutionRequestDto userSolutionRequestDto, CancellationToken cancellationToken = default)
    {
        return await userSolutionRepository.GetAllUserSolutionsAsync(userSolutionRequestDto, cancellationToken);
    }
}