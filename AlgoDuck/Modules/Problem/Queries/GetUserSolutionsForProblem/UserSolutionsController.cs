using AlgoDuck.Modules.Item.Queries.GetAllDucksPaged;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.GetUserSolutionsForProblem;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UserSolutionsController(
    IUserSolutionService userSolutionService
    ) : ControllerBase
{
    public async Task<IActionResult> GetAllUserSolutionsAsync([FromQuery] Guid problemId, [FromQuery] int pageSize, [FromQuery] int currentPage, CancellationToken cancellationToken)
    {
        
        Console.WriteLine("huh");
        var userIdResult = User.GetUserId();
        if (userIdResult.IsErr)
            return userIdResult.ToActionResult();

        var userSolutionResult = await userSolutionService.GetAllUserSolutionsAsync(new UserSolutionRequestDto
        {
            ProblemId = problemId,
           PagedRequestWithAttribution = new PagedRequestWithAttribution
           {
               CurrPage = currentPage,
               PageSize = pageSize,
               UserId = userIdResult.AsOk
           }
        }, cancellationToken);

        return userSolutionResult.ToActionResult();
    }
}



public class UserSolutionDto
{
    public required Guid UserSolutionId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required long CodeRuntimeSubmitted { get; set; }
}

public class UserSolutionRequestDto
{
    public required PagedRequestWithAttribution PagedRequestWithAttribution { get; set; }
    public required Guid ProblemId { get; set; }
}