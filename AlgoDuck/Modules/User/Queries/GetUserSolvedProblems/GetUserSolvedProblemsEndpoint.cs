using System.Security.Claims;
using AlgoDuck.Shared.Http;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserSolvedProblems;

[ApiController]
[Route("api/user/solved-problems")]
[Authorize]
public sealed class GetUserSolvedProblemsEndpoint : ControllerBase
{
    private readonly IGetUserSolvedProblemsHandler _handler;
    private readonly IValidator<GetUserSolvedProblemsQuery> _validator;

    public GetUserSolvedProblemsEndpoint(
        IGetUserSolvedProblemsHandler handler,
        IValidator<GetUserSolvedProblemsQuery> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] GetUserSolvedProblemsQuery query,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            var message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            return BadRequest(new StandardApiResponse
            {
                Status = Status.Error,
                Message = message
            });
        }

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Unauthorized"
            });
        }

        var solvedProblems = await _handler.HandleAsync(userId, query, cancellationToken);

        return Ok(new StandardApiResponse<IReadOnlyList<UserSolvedProblemsDto>>
        {
            Status = Status.Success,
            Body = solvedProblems
        });
    }
}