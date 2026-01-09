using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetUserEditorPreferences;

public class UserEditorPreferencesController : ControllerBase
{
    [HttpGet]
    [Authorize]
    [Route("api/user/preferences/editor")]
    public async Task<IActionResult> GetUserEditorPreferencesAsync()
    {
        throw new NotImplementedException();
    }   
}

public interface IGetUserEditorPreferencesService
{
    public Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetUserEditorPreferencesService : IGetUserEditorPreferencesService
{
    private readonly IGetUserEditorPreferencesRepository _repository;

    public GetUserEditorPreferencesService(IGetUserEditorPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetUserEditorPreferencesAsync(userId, cancellationToken);
    }
}

public interface IGetUserEditorPreferencesRepository
{
    public Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId, CancellationToken cancellationToken = default);
}

public class GetUserEditorPreferencesRepository : IGetUserEditorPreferencesRepository
{
    private readonly ApplicationQueryDbContext _dbContext;
    

    public GetUserEditorPreferencesRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId, CancellationToken cancellationToken = default)
    {/**/
        var selectedLayout = await _dbContext.OwnsLayouts.Where(o => o.IsSelected && userId == o.UserId).FirstOrDefaultAsync(cancellationToken: cancellationToken);
        var editorPreferences = await _dbContext.UserConfigs.Include(c => c.EditorTheme).Where(u => u.UserId == userId).Select(c => new
        {
            FontSize = c.EditorFontSize,
            EditorTheme = new EditorThemeDto{
                ThemeId = c.EditorThemeId,
                ThemeName = c.EditorTheme.ThemeName,
            }
        }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        throw new NotImplementedException();
    }
}

public class UserEditorPreferencesDto
{
    public required LayoutDto Layout { get; set; }
    public required EditorThemeDto Theme { get; set; }
    public required int FontSize { get; set; }
}

public class LayoutDto
{
    public required Guid? LayoutId { get; set; }
    public required string LayoutName { get; set; }
    public required object LayoutContent { get; set; }
}

public class EditorThemeDto
{
    public required Guid ThemeId { get; set; }
    public required string ThemeName { get; set; }
}
