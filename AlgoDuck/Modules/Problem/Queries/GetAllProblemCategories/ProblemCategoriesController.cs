using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetAllProblemCategories;

[ApiController]
[Route("api/category/all")]
[Authorize]
public class ProblemCategoriesController : ControllerBase
{
    private readonly IProblemCategoriesService _service;

    public ProblemCategoriesController(IProblemCategoriesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCategoriesAsync(CancellationToken cancellationToken)
    {
        return await _service.GetAllAsync(cancellationToken).ToActionResultAsync();
    } 
}