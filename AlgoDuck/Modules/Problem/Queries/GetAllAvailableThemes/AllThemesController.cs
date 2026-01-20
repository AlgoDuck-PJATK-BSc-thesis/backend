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


public class ThemeDto
{
    public required Guid ThemeId { get; set; }
    public required string ThemeName { get; set; }
}