using System.Text.Json;
using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Http;
using FluentValidation;
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
    private readonly IValidator<UpsertProblemDto> _validator;

   public UpdateProblemController(IUpdateProblemService updateProblemService, IValidator<UpsertProblemDto> validator)
   {
      _updateProblemService = updateProblemService;
      _validator = validator;
   }

   [HttpPut]
   public async Task<IActionResult> UpdateProblemAsync(
      [FromBody] UpsertProblemDto upsertProblemDto,
      [FromQuery] Guid problemId,
      CancellationToken cancellationToken = default)
   {
      var validationResult = await  _validator.ValidateAsync(upsertProblemDto, cancellationToken);
      if (!validationResult.IsValid)
         return BadRequest(validationResult.Errors);
      return await User
         .GetUserId()
         .BindAsync(async userId =>
         {
            upsertProblemDto.RequestingUserId = userId;
            return await _updateProblemService.UpdateProblemAsync(upsertProblemDto, problemId, cancellationToken);
         }).ToActionResultAsync();
   }
}


