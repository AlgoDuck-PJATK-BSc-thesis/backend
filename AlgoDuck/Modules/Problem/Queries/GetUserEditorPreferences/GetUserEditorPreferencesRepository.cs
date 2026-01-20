using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.GetUserEditorPreferences;

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

        return Result<UserEditorPreferencesDto, ErrorObject<string>>.Ok(new UserEditorPreferencesDto
        {
            FontSize = editorPreferences?.FontSize ?? 11,
            Theme = new ThemeDto
            {
                ThemeId = editorPreferences?.EditorTheme.ThemeId ?? Guid.Parse("276cc32e-a0bd-408e-b6f0-0f4e3ff80796"),
                ThemeName = editorPreferences?.EditorTheme.ThemeName ?? "vs-dark",
            },
            Layout = new LayoutDto
            {
                LayoutId = layoutId,
                LayoutName = selectedLayout?.Layout.LayoutName ?? "default",
                LayoutContent = layoutResult.AsOk!
            }
        });
    }
}