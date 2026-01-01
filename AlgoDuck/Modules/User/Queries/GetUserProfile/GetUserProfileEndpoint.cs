using System.Security.Claims;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.User.Queries.GetUserProfile;

[ApiController]
[Route("api/user/profile")]
[Authorize]
public sealed class GetUserProfileEndpoint : ControllerBase
{
    private readonly IGetUserProfileHandler _handler;

    public GetUserProfileEndpoint(IGetUserProfileHandler handler)
    {
        _handler = handler;
    }

    private static string[] ExtractRoles(ClaimsPrincipal user)
    {
        return user.Claims
            .Where(c => c.Type == ClaimTypes.Role || c.Type == "role" || c.Type == "roles")
            .Select(c => (c.Value).Trim())
            .Where(v => !string.IsNullOrWhiteSpace(v))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
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

        var profile = await _handler.HandleAsync(userId, cancellationToken);

        var roles = ExtractRoles(User);
        var primaryRole = roles.FirstOrDefault();

        return Ok(new StandardApiResponse<GetUserProfileResponseDto>
        {
            Body = new GetUserProfileResponseDto    
            {
                Profile = profile,
                Roles = roles,
                PrimaryRole = primaryRole
            }
        });
    }
}