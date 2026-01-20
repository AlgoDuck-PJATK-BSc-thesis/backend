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