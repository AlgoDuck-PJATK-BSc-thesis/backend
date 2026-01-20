using System.Text.Json;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpdateProblem;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
[EnableRateLimiting("CodeExecution")]
public class UpdateProblemController : ControllerBase
{
   private readonly IUpdateProblemService _updateProblemService;

   public UpdateProblemController(IUpdateProblemService updateProblemService)
   {
      _updateProblemService = updateProblemService;
   }

   [HttpPut]
   public async Task<IActionResult> UpdateProblemAsync(
      [FromBody] UpsertProblemDto updateProblemDto,
      [FromQuery] Guid problemId,
      CancellationToken cancellationToken = default)
   {
      Console.WriteLine(JsonSerializer.Serialize(updateProblemDto));
      return await User
         .GetUserId()
         .BindAsync(async userId =>
         {
            updateProblemDto.RequestingUserId = userId;
            return await _updateProblemService.UpdateProblemAsync(updateProblemDto, problemId, cancellationToken);
         }).ToActionResultAsync();
   }
}


