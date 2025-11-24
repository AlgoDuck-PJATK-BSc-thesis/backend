using System.Net;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Email;
using AlgoDuck.Modules.Auth.DTOs;
using AlgoDuck.Shared.Http;
using Status = AlgoDuck.Shared.Http.Status;

namespace AlgoDuck.Modules.Auth.Controllers
{
    [ApiController]
    [Route("api/auth/email")]
    public sealed class EmailVerificationController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly IEmailSender _email;
        private readonly ILogger<EmailVerificationController> _log;

        public EmailVerificationController(
            UserManager<ApplicationUser> users,
            IEmailSender email,
            ILogger<EmailVerificationController> log)
        {
            _users = users;
            _email = email;
            _log = log;
        }

        [HttpPost("send")]
        [Authorize]
        public async Task<IActionResult> Send([FromBody] EmailVerificationStartDto dto, CancellationToken ct)
        {
            var user = await _users.GetUserAsync(User);
            if (user is null) return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "No such user"
            });
            
            if (string.IsNullOrWhiteSpace(user.Email))
                return BadRequest(new StandardApiResponse
                {
                    Status = Status.Error,
                    Message = "Email not set"
                });
            
            if (await _users.IsEmailConfirmedAsync(user))
                return Ok(new StandardApiResponse
                {
                    Message = "Already verified"
                });

            var token = await _users.GenerateEmailConfirmationTokenAsync(user);
            var encoded = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var returnUrl = string.IsNullOrWhiteSpace(dto?.ReturnUrl) ? "/" : dto.ReturnUrl;
            var confirmUrl = $"{returnUrl}?email_confirm=1&userId={user.Id}&token={WebUtility.UrlEncode(encoded)}";

            var subject = "Verify your email";
            var text = $"Click to verify: {confirmUrl}";
            var html = $"<p>Click to verify: <a href=\"{WebUtility.HtmlEncode(confirmUrl)}\">Verify</a></p>";

            await _email.SendAsync(user.Email!, subject, text, html, ct);
            _log.LogInformation("email_verification_sent user={UserId}", user.Id);

            return Ok(new StandardApiResponse
            {
                Message = "Verification Sent"
            });
        }

        [HttpPost("confirm")]
        [AllowAnonymous]
        public async Task<IActionResult> Confirm([FromQuery] Guid userId, [FromQuery] string token, CancellationToken ct)
        {
            var user = await _users.FindByIdAsync(userId.ToString());
            if (user is null) return BadRequest(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "No such user"
            });

            try
            {
                var decoded = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(token));
                var res = await _users.ConfirmEmailAsync(user, decoded);
                if (!res.Succeeded)
                    return BadRequest(new StandardApiResponse
                    {
                        Status = Status.Error,
                        Message = string.Join("; ", res.Errors.Select(e => e.Description))
                    });

                return Ok(new StandardApiResponse
                {
                    Message = "Email verified"
                });
            }
            catch
            {
                return BadRequest(new StandardApiResponse
                {
                    Status = Status.Error,
                    Message = "Invalid or malformed token"
                });
            }
        }
    }
}