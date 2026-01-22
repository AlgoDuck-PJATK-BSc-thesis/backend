using AlgoDuck.Modules.Item.Queries.GetOwnedUsedItemsByUserId;
using AlgoDuck.Modules.Problem.Commands.ProblemUpsert.UpsertTypes;
using AlgoDuck.Shared.Http;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoDuck.Modules.Problem.Commands.ProblemUpsert.CreateProblem;

[Authorize(Roles = "admin")]
[ApiController]
[Route("api/[controller]")]
[EnableRateLimiting("CodeExecution")]
public class CreateProblemController : ControllerBase
{
    private readonly ICreateProblemService _createProblemService;
    private readonly IValidator<UpsertProblemDto> _validator;


    public CreateProblemController(ICreateProblemService createProblemService, IValidator<UpsertProblemDto> validator)
    {
        _createProblemService = createProblemService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<IActionResult> CreateProblemAsync([FromBody] UpsertProblemDto upsertProblemDto,
        CancellationToken cancellation)
    {
        var validationResult = await  _validator.ValidateAsync(upsertProblemDto, cancellation);
        if (!validationResult.IsValid)
            return BadRequest(validationResult.Errors);
        
        return await User.GetUserId()
            .BindAsync(async userId =>
            {
                upsertProblemDto.RequestingUserId = userId;
                return await _createProblemService.CreateProblemAsync(upsertProblemDto, cancellation);
            }).ToActionResultAsync();
    }
}



