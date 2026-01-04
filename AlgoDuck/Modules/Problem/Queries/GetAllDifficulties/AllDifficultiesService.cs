using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetAllDifficulties;

public interface IAllDifficultiesService
{
    public Task<Result<ICollection<DifficultyDto>, ErrorObject<string>>> GetAllDifficultiesAsync(CancellationToken cancellationToken = default);
}

public class AllDifficultiesService(
    IAllDifficultiesRepository allDifficultiesRepository
    ) : IAllDifficultiesService
{
    public async Task<Result<ICollection<DifficultyDto>, ErrorObject<string>>> GetAllDifficultiesAsync(CancellationToken cancellationToken = default)
    {
        return await allDifficultiesRepository.GetAllDifficultiesAsync(cancellationToken);
    }
}