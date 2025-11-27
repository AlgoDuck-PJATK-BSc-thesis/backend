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
            HttpContext.Request.Cookies.TryGetValue("refresh_token", out var refresh);

            await _authService.RefreshTokenAsync(new RefreshDto
            {
                RefreshToken = refresh
            }, Response, ct);
            
            return Ok(new StandardApiResponse
            {
                Message = "token refreshed"
            });
        }

        [HttpPost("refresh-body")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshFromBody([FromBody] RefreshDto dto, CancellationToken ct)
        {
            await _authService.RefreshTokenAsync(dto, Response, ct);
            
            return Ok(new StandardApiResponse
            {
                Message = "token refreshed"
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Unauthorized(new StandardApiResponse
                {
                    Status = Status.Error,
                });
            }
            await _authService.LogoutAsync(Guid.Parse(uid), ct);
            
            return Ok(new StandardApiResponse
            {
                Message = "logged out"
            });
        }
    }
}