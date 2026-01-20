using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetAllAvailableThemes;

[ApiController]
[Authorize]
[Route("api/problem/editor/theme")]
public class AllThemesController : ControllerBase
{
    private readonly IAllThemesService _allThemesService;

    public AllThemesController(IAllThemesService allThemesService)
    {
        _allThemesService = allThemesService;
    }

    public async Task<IActionResult> GetAllThemesAsync()
    {
        return await _allThemesService.GetAllThemesAsync().ToActionResultAsync();
    }
}

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


public class ThemeDto
{
    public required Guid ThemeId { get; set; }
    public required string ThemeName { get; set; }
}