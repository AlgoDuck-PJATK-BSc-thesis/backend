using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.UpdateEditorPreferences;

public class UpdateEditorPreferencesController : ControllerBase
{
    private readonly IUpdateEditorPreferencesService _service;

    public UpdateEditorPreferencesController(IUpdateEditorPreferencesService service)
    {
        _service = service;
    }

    [HttpPatch]
    [Route("api/problem/editor/theme")]
    [Authorize]
    public async Task<IActionResult> UpdateUserEditorPreferencesAsync([FromBody] PreferencesUpdateRequestDto request,
        CancellationToken cancellationToken = default)
    {
        return await User.GetUserId().BindAsync(async userId =>
        {
            request.UserId = userId;
            return await _service.UpdateEditorPreferencesAsync(request, cancellationToken);
        }).ToActionResultAsync();
    }
}

public interface IUpdateEditorPreferencesService
{
    public Task<Result<PreferencesUpdateResultDto, ErrorObject<string>>> UpdateEditorPreferencesAsync(
        PreferencesUpdateRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class UpdateEditorPreferencesService : IUpdateEditorPreferencesService
{
    private readonly IUpdateEditorPreferencesRepository _repository;

    public UpdateEditorPreferencesService(IUpdateEditorPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PreferencesUpdateResultDto, ErrorObject<string>>> UpdateEditorPreferencesAsync(
        PreferencesUpdateRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateEditorPreferencesAsync(requestDto, cancellationToken);
    }
}

public interface IUpdateEditorPreferencesRepository
{
    public Task<Result<PreferencesUpdateResultDto, ErrorObject<string>>> UpdateEditorPreferencesAsync(
        PreferencesUpdateRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class UpdateEditorPreferencesRepository : IUpdateEditorPreferencesRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public UpdateEditorPreferencesRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PreferencesUpdateResultDto, ErrorObject<string>>> UpdateEditorPreferencesAsync(
        PreferencesUpdateRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        var userConfig = await _dbContext.UserConfigs.Where(uc => uc.UserId == requestDto.UserId)
            .FirstOrDefaultAsync(uc => uc.UserId == requestDto.UserId, cancellationToken);

        userConfig ??= new UserConfig
        {
            EmailNotificationsEnabled = false,
            IsDarkMode = true,
            PushNotificationsEnabled = false,
            UserId = requestDto.UserId,
            IsHighContrast = false,
            Language = "en"
        };

        if (_dbContext.EditorThemes.Select(et => et.EditorThemeId).Any(et => et == requestDto.ThemeId) &&
            requestDto.ThemeId is not null)
            userConfig.EditorThemeId = requestDto.ThemeId.Value;

        var rowsUpdated = await _dbContext.OwnsLayouts
            .Where(u => u.UserId == requestDto.UserId && u.LayoutId == requestDto.LayoutId)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.IsSelected, true), cancellationToken: cancellationToken);

        if (requestDto.FontSize is not null)
            userConfig.EditorFontSize = requestDto.FontSize.Value;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result<PreferencesUpdateResultDto, ErrorObject<string>>.Ok(new PreferencesUpdateResultDto
        {
            FontSize = userConfig.EditorFontSize,
            ThemeId = userConfig.EditorThemeId
        });
    }
}

public class PreferencesUpdateResultDto
{
    public int FontSize { get; set; }
    public Guid LayoutId { get; set; }
    public Guid ThemeId { get; set; }
}

public class PreferencesUpdateRequestDto
{
    internal Guid UserId { get; set; }
    public int? FontSize { get; set; }
    public Guid? LayoutId { get; set; }
    public Guid? ThemeId { get; set; }
}