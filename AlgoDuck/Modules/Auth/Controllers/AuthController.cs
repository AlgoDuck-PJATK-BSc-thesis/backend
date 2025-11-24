using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using AlgoDuck.Modules.Auth.DTOs;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace AlgoDuck.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting("AuthTight")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly JwtSettings _jwt;

        public AuthController(IAuthService authService, IOptions<JwtSettings> jwtOptions)
        {
            _authService = authService;
            _jwt = jwtOptions.Value;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto, CancellationToken cancellationToken)
        {
            await _authService.RegisterAsync(dto, cancellationToken);
            return Ok(new StandardApiResponse
            {
                Message = "User registered successfully."
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken cancellationToken)
        {
            await _authService.LoginAsync(dto, Response, cancellationToken);
            return Ok(new StandardApiResponse
            {
                Message = "Logged in successfully."
            });
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh(CancellationToken cancellationToken)
        {
            await _authService.RefreshTokenAsync(new RefreshDto { RefreshToken = string.Empty }, Response, cancellationToken);
            return Ok(new StandardApiResponse
            {
                Message = "Token refreshed successfully."
            });
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized(new StandardApiResponse
                {
                    Status = Status.Error,
                });
            }
            
            await _authService.LogoutAsync(Guid.Parse(userId), cancellationToken);

            var domain = string.IsNullOrWhiteSpace(_jwt.CookieDomain) ? null : _jwt.CookieDomain;
            Response.Cookies.Delete(_jwt.JwtCookieName, new CookieOptions { Path = "/", Domain = domain });
            Response.Cookies.Delete(_jwt.RefreshCookieName, new CookieOptions { Path = "/api/auth/refresh", Domain = domain });
            Response.Cookies.Delete(_jwt.CsrfCookieName, new CookieOptions { Path = "/", Domain = domain });

            
            return Ok(new StandardApiResponse
            {
                Message = "Logged out successfully."
            });
        }
    }
}