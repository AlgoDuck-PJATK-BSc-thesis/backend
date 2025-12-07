using System.Security.Claims;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserAchievements;

[ApiController]
[Route("api/user/achievements")]
[Authorize]
public sealed class GetUserAchievementsEndpoint : ControllerBase
{
    private readonly IGetUserAchievementsHandler _handler;
    private readonly GetUserAchievementsValidator _validator;

    public GetUserAchievementsEndpoint(IGetUserAchievementsHandler handler, GetUserAchievementsValidator validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] GetUserAchievementsRequestDto requestDto, CancellationToken cancellationToken)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Unauthorized"
            });
        }

        var validationResult = await _validator.ValidateAsync(requestDto, cancellationToken);
        if (!validationResult.IsValid)
        {
            return BadRequest(new StandardApiResponse
            {
                Status = Status.Error,
                Message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage))
            });
        }

        var achievements = await _handler.HandleAsync(userId, requestDto, cancellationToken);

        return Ok(new StandardApiResponse<IReadOnlyList<UserAchievementDto>>
        {
            Body = achievements
        });
    }
}