using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetAllDifficulties;

[Authorize]
[Route("api/[controller]")]
[ApiController]
public class AllDifficultiesController(
    IAllDifficultiesService allDifficultiesService
    ) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllDifficultiesAsync()
    {
        return await allDifficultiesService
            .GetAllDifficultiesAsync()
            .ToActionResultAsync();
    }
}

public class DifficultyDto
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
}