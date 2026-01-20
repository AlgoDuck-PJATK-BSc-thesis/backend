using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetAllAvailableThemes;

public interface IAllThemesService
{
    public Task<Result<ICollection<ThemeDto>, ErrorObject<string>>> GetAllThemesAsync(CancellationToken cancellationToken = default);
}

public class AllThemesService : IAllThemesService
{
    private readonly IAllThemesRepository _allThemesRepository;

    public AllThemesService(IAllThemesRepository allThemesRepository)
    {
        _allThemesRepository = allThemesRepository;
    }

    public Task<Result<ICollection<ThemeDto>, ErrorObject<string>>> GetAllThemesAsync(CancellationToken cancellationToken = default)
    {
        return _allThemesRepository.GetAllThemesAsync(cancellationToken);
    }
}