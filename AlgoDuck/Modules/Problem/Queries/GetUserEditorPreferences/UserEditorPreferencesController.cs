using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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



public class UserEditorPreferencesDto
{
    public required LayoutDto Layout { get; set; }
    public required ThemeDto Theme { get; set; }
    public required int FontSize { get; set; }
}

public class LayoutDto
{
    public required Guid? LayoutId { get; set; }
    public required string LayoutName { get; set; }
    public required object LayoutContent { get; set; }
}

public class ThemeDto
{
    public required Guid ThemeId { get; set; }
    public required string ThemeName { get; set; }
}

public class EditorThemeDto
{
    public required Guid ThemeId { get; set; }
    public required string ThemeName { get; set; }
}