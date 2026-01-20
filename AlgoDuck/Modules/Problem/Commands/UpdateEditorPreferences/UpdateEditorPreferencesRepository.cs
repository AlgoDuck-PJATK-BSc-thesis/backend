using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.UpdateEditorPreferences;

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
            UserId = requestDto.UserId,
            IsHighContrast = false,
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
