using System.Security.Claims;
using AlgoDuck.Modules.Auth.DTOs;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public sealed class AuthSessionController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthSessionController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshFromCookie(CancellationToken ct)
        {
            await _authService.RefreshTokenAsync(new RefreshDto(), Response, ct);
            return Ok(ApiResponse.Success(new { refreshed = true }));
        }

        [HttpPost("refresh-body")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshFromBody([FromBody] RefreshDto dto, CancellationToken ct)
        {
            await _authService.RefreshTokenAsync(dto, Response, ct);
            return Ok(ApiResponse.Success(new { refreshed = true }));
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(uid))
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            await _authService.LogoutAsync(Guid.Parse(uid), ct);
            return Ok(ApiResponse.Success(new { loggedOut = true }));
        }
    }
}