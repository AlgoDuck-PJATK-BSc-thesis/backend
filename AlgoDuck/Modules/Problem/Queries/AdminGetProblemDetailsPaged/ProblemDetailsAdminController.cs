using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetProblemDetailsPaged;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/[controller]")]
public class ProblemDetailsAdminController : ControllerBase
{
    
    private readonly IPagedProblemDetailsAdminService _allPagedProblemsPagedService;

    public ProblemDetailsAdminController(IPagedProblemDetailsAdminService allPagedProblemsPagedService)
    {
        _allPagedProblemsPagedService = allPagedProblemsPagedService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllItemsPagedAsync(
        [FromQuery] ColumnFilterRequest<FetchableColumn> query,
        [FromQuery] int currentPage,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        return await User.GetUserId()
            .BindAsync(async userId => await _allPagedProblemsPagedService.GetProblemDetailsAsync(new PagedRequestWithAttribution<ColumnFilterRequest<FetchableColumn>>
            {
                UserId = userId,
                CurrPage = currentPage,
                PageSize = pageSize,
                FurtherData = query
            }, cancellationToken))
            .ToActionResultAsync();
    }
}





public enum FetchableColumn{
    Name,
    CreatedOn,
    CreatedBy,
    ProblemId,
    Category,
    Difficulty,
    CompletionRatio
}

public class ProblemDetailsDto
{
    public string? Name { get; set; }
    public DateTime? CreatedOn { get; set; }
    public CreatingUserDto? CreatedBy { get; set; }
    public Guid ProblemId { get; set; }
    public double? CompletionRatio { get; set; }
    public CategoryDto? Category { get; set; }
    public DifficultyDto? Difficulty { set; get; }
}

public class DifficultyDto
{
    public required Guid DifficultyId { get; set; }
    public required string Name { get; set; }   
}

public class CategoryDto
{
    public required Guid CategoryId { get; set; }
    public required string Name { get; set; }
}

public class CreatingUserDto
{
    public required Guid Id { get; set; }
    public required string Username { get; set; }
}