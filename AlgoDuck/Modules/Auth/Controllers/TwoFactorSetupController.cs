using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using AlgoDuck.Models.User;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth/2fa")]
    [Authorize]
    public sealed class TwoFactorSetupController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public TwoFactorSetupController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("status")]
        public async Task<IActionResult> Status(CancellationToken ct)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(uid))
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            return Ok(ApiResponse.Success(new
            {
                enabled = user.TwoFactorEnabled,
                method = user.TwoFactorEnabled ? "email" : null
            }));
        }

        [HttpPost("enable")]
        public async Task<IActionResult> Enable(CancellationToken ct)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(uid))
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            if (string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
                return BadRequest(ApiResponse.Fail("Email must be set and verified to enable 2FA.", "email_unverified"));

            if (!user.TwoFactorEnabled)
            {
                user.TwoFactorEnabled = true;
                var res = await _userManager.UpdateAsync(user);
                if (!res.Succeeded)
                    return BadRequest(ApiResponse.Fail(string.Join("; ", res.Errors.Select(e => e.Description)), "update_failed"));
            }

            return Ok(ApiResponse.Success(new { enabled = true, method = "email" }));
        }

        [HttpPost("disable")]
        public async Task<IActionResult> Disable(CancellationToken ct)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(uid))
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
                return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

            if (user.TwoFactorEnabled)
            {
                user.TwoFactorEnabled = false;
                var res = await _userManager.UpdateAsync(user);
                if (!res.Succeeded)
                    return BadRequest(ApiResponse.Fail(string.Join("; ", res.Errors.Select(e => e.Description)), "update_failed"));
            }

            return Ok(ApiResponse.Success(new { enabled = false }));
        }
    }
}