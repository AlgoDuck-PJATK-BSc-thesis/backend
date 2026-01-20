using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetAllDifficulties;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AllDifficultiesController : ControllerBase
{
    private readonly IAllDifficultiesService _allDifficultiesService;

    public AllDifficultiesController(IAllDifficultiesService allDifficultiesService)
    {
        _allDifficultiesService = allDifficultiesService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllDifficultiesAsync()
    {
        return await _allDifficultiesService
            .GetAllDifficultiesAsync()
            .ToActionResultAsync();
    }
}

public class DifficultyDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}