using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AlgoDuck.Modules.User.DTOs;
using AlgoDuck.Modules.User.Interfaces;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.User.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            var profile = await _userService.GetProfileAsync(Guid.Parse(userId), cancellationToken);
            return Ok(ApiResponse.Success(profile));
        }

        [HttpPut("me")]
        public async Task<IActionResult> UpdateMyProfile([FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            await _userService.UpdateProfileAsync(Guid.Parse(userId), dto, cancellationToken);
            return Ok(ApiResponse.Success(new { message = "Profile updated successfully." }));
        }
    }
}