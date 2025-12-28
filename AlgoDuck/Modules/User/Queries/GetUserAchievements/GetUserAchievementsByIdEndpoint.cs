using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserAchievements;

[ApiController]
[Route("api/user/{userId:guid}/achievements")]
[Authorize]
public sealed class GetUserAchievementsByIdEndpoint : ControllerBase
{
    private readonly IGetUserAchievementsHandler _handler;
    private readonly GetUserAchievementsValidator _validator;

    public GetUserAchievementsByIdEndpoint(IGetUserAchievementsHandler handler, GetUserAchievementsValidator validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid userId, [FromQuery] GetUserAchievementsRequestDto requestDto, CancellationToken cancellationToken)
    {
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