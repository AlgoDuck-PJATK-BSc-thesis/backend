using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.CreateProblem;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class CreateProblemController(
    ICreateProblemService createProblemService
    ) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateProblemAsync([FromBody] UpsertProblemDto upsertProblemDto,
        CancellationToken cancellation)
    {
        Console.WriteLine("Hit CreateProblemAsync");
        return await User.GetUserId()
            .BindAsync(async userId =>
            {
                upsertProblemDto.RequestingUserId = userId;
                return await createProblemService.CreateProblemAsync(upsertProblemDto, cancellation);
            }).ToActionResultAsync();
    }
}



