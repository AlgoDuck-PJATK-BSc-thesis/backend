using AlgoDuck.DAL;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetUserEditorPreferences;

public class UserEditorPreferencesController : ControllerBase
{
    private readonly IGetUserEditorPreferencesService _service;

    public UserEditorPreferencesController(IGetUserEditorPreferencesService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize]
    [Route("api/user/preferences/editor")]
    public async Task<IActionResult> GetUserEditorPreferencesAsync(CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId => await _service.GetUserEditorPreferencesAsync(userId, cancellationToken))
            .ToActionResultAsync();
    }
}

public interface IGetUserEditorPreferencesService
{
    public Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId,
        CancellationToken cancellationToken = default);
}

public class GetUserEditorPreferencesService : IGetUserEditorPreferencesService
{
    private readonly IGetUserEditorPreferencesRepository _repository;

    public GetUserEditorPreferencesService(IGetUserEditorPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetUserEditorPreferencesAsync(userId, cancellationToken);
    }
}

public interface IGetUserEditorPreferencesRepository
{
    public Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId,
        CancellationToken cancellationToken = default);
}

public class GetUserEditorPreferencesRepository : IGetUserEditorPreferencesRepository
{
    private readonly ApplicationQueryDbContext _dbContext;
    private readonly IAwsS3Client _s3Client;


    public GetUserEditorPreferencesRepository(ApplicationQueryDbContext dbContext, IAwsS3Client s3Client)
    {
        _dbContext = dbContext;
        _s3Client = s3Client;
    }

    public async Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        var selectedLayout = await _dbContext.OwnsLayouts.Include(l => l.Layout)
            .Where(o => o.IsSelected && userId == o.UserId)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var layoutId =
            selectedLayout?.LayoutId ??
            Guid.Parse(
                "7d2e1c42-f7da-4261-a8c1-42826d976116"); /* hardcoded, default, seeded layout id, probs should be read from a config */

        var layoutResult = await _s3Client.GetJsonObjectByPathAsync<object>($"users/layouts/{layoutId}.json",
            cancellationToken: cancellationToken);

        if (layoutResult.IsErr)
            return Result<UserEditorPreferencesDto, ErrorObject<string>>.Err(
                ErrorObject<string>.NotFound($"Could not load layout"));

        var editorPreferences = await _dbContext.UserConfigs.Include(c => c.EditorTheme).Where(u => u.UserId == userId)
            .Select(c => new
            {
                FontSize = c.EditorFontSize,
                EditorTheme = new EditorThemeDto
                {
                    ThemeId = c.EditorThemeId,
                    ThemeName = c.EditorTheme.ThemeName,
                }
            }).FirstOrDefaultAsync(cancellationToken: cancellationToken);

        return Result<UserEditorPreferencesDto, ErrorObject<string>>.Ok(new UserEditorPreferencesDto()
        {
            FontSize = editorPreferences?.FontSize ?? 11,
            ThemeName = editorPreferences?.EditorTheme.ThemeName ?? "vs-dark",
            Layout = new LayoutDto
            {
                LayoutId = layoutId,
                LayoutName = selectedLayout?.Layout.LayoutName ?? "default",
                LayoutContent = layoutResult.AsOk!
            }
        });
    }
}

public class UserEditorPreferencesDto
{
    public required LayoutDto Layout { get; set; }
    public required string ThemeName { get; set; }
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