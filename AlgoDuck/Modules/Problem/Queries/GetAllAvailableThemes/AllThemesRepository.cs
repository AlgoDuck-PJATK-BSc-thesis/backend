using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllAvailableThemes;

public interface IAllThemesRepository
{
    public Task<Result<ICollection<ThemeDto>, ErrorObject<string>>> GetAllThemesAsync(CancellationToken cancellationToken = default);
}

public class AllThemesRepository : IAllThemesRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public AllThemesRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<ICollection<ThemeDto>, ErrorObject<string>>> GetAllThemesAsync(CancellationToken cancellationToken = default)
    {
        return Result<ICollection<ThemeDto>, ErrorObject<string>>.Ok(await _dbContext.EditorThemes.Select(t => new ThemeDto()
        {
            ThemeName = t.ThemeName,
            ThemeId = t.EditorThemeId
        }).ToListAsync(cancellationToken: cancellationToken));
    }
}

