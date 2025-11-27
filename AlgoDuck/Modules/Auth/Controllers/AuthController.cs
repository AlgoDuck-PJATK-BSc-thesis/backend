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
    }
}