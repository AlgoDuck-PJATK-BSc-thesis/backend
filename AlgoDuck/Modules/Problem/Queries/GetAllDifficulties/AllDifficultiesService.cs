using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetAllDifficulties;

public interface IAllDifficultiesService
{
    public Task<Result<ICollection<DifficultyDto>, ErrorObject<string>>> GetAllDifficultiesAsync(CancellationToken cancellationToken = default);
}

public class AllDifficultiesService : IAllDifficultiesService
{
    private readonly IAllDifficultiesRepository _allDifficultiesRepository;

    public AllDifficultiesService(IAllDifficultiesRepository allDifficultiesRepository)
    {
        _allDifficultiesRepository = allDifficultiesRepository;
    }

    public async Task<Result<ICollection<DifficultyDto>, ErrorObject<string>>> GetAllDifficultiesAsync(CancellationToken cancellationToken = default)
    {
        return await _allDifficultiesRepository.GetAllDifficultiesAsync(cancellationToken);
    }
}