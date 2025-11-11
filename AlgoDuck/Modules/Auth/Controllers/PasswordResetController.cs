using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.DTOs;
using AlgoDuck.Modules.Auth.Email;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth/password")]
    public sealed class PasswordResetController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IEmailSender _email;
        private readonly ILogger<PasswordResetController> _log;

        public PasswordResetController(
            UserManager<ApplicationUser> users,
            IEmailSender email,
            ILogger<PasswordResetController> log)
        {
            _users = users;
            _email = email;
            _log = log;
        }

        [HttpPost("forgot")]
        [AllowAnonymous]
        public async Task<IActionResult> Forgot([FromBody] PasswordForgotDto dto, CancellationToken ct)
        {
            var user = string.IsNullOrWhiteSpace(dto.Email) ? null : await _users.FindByEmailAsync(dto.Email);
            if (user != null && await _users.IsEmailConfirmedAsync(user))
            {
                var token = await _users.GeneratePasswordResetTokenAsync(user);
                var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                var returnUrl = string.IsNullOrWhiteSpace(dto.ReturnUrl) ? "/" : dto.ReturnUrl;
                var resetUrl = $"{returnUrl}?reset=1&userId={user.Id}&token={WebUtility.UrlEncode(encoded)}";

                var subject = "Reset your password";
                var text = $"Reset link: {resetUrl}";
                var html = $"<p>Reset link: <a href=\"{WebUtility.HtmlEncode(resetUrl)}\">Reset password</a></p>";

                await _email.SendAsync(user.Email!, subject, text, html, ct);
                _log.LogInformation("password_reset_email_sent user={UserId}", user.Id);
            }

            return Ok(ApiResponse.Success(new { message = "ok" }));
        }

        [HttpPost("reset")]
        [AllowAnonymous]
        public async Task<IActionResult> Reset([FromBody] PasswordResetDto dto, CancellationToken ct)
        {
            var user = await _users.FindByIdAsync(dto.UserId.ToString());
            if (user is null) return BadRequest(ApiResponse.Fail("Invalid user.", "invalid_user"));

            try
            {
                var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(dto.Token));
                var res = await _users.ResetPasswordAsync(user, decoded, dto.NewPassword);
                if (!res.Succeeded)
                    return BadRequest(ApiResponse.Fail(string.Join("; ", res.Errors.Select(e => e.Description)), "reset_failed"));

                return Ok(ApiResponse.Success(new { message = "password_reset" }));
            }
            catch
            {
                return BadRequest(ApiResponse.Fail("Invalid or malformed token.", "invalid_token"));
            }
        }
    }
}