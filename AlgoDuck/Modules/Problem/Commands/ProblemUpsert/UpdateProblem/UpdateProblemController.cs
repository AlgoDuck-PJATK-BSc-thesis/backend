using System.Text.Json;
using AlgoDuck.Modules.Item.Queries.GetOwnedItemsByUserId;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpdateProblem;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "admin")]
public class UpdateProblemController(
   IUpdateProblemService updateProblemService
   ) : ControllerBase
{
   [HttpPut]
   public async Task<IActionResult> UpdateProblemAsync(
      [FromBody] UpsertProblemDto updateProblemDto,
      [FromQuery] Guid problemId,
      CancellationToken cancellationToken = default)
   {
      return await User
         .GetUserId()
         .BindAsync(async userId =>
         {
            updateProblemDto.RequestingUserId = userId;
            return await updateProblemService.UpdateProblemAsync(updateProblemDto, problemId, cancellationToken);
         }).ToActionResultAsync();
   }
}


