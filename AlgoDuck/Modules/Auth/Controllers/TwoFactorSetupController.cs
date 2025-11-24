using System.Security.Claims;
using AlgoDuck.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
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
                return Unauthorized(new StandardApiResponse
                {
                    Status = Shared.Http.Status.Error,
                    Message = "Unauthorized"
                });

            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
                return Unauthorized(new StandardApiResponse()
                {
                    Status = Shared.Http.Status.Error,
                    Message = "Unauthorized"
                });

            return Ok(new StandardApiResponse<StatusResponseDto>
            {
                Body = new StatusResponseDto
                {
                    Enabled = user.TwoFactorEnabled,
                    Method = user.TwoFactorEnabled ? "email" : null
                }
            });
        }

        private sealed class StatusResponseDto
        {
            public required bool Enabled { get; set; }
            public string? Method { get; set; }
        }

        [HttpPost("enable")]
        public async Task<IActionResult> Enable(CancellationToken ct)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Unauthorized(new StandardApiResponse
                {
                    Status = Shared.Http.Status.Error,
                    Message = "Unauthorized"
                });
            }
            
            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
            {
                return Unauthorized(new StandardApiResponse()
                {
                    Status = Shared.Http.Status.Error,
                    Message = "Unauthorized"
                });
            }

            if (string.IsNullOrWhiteSpace(user.Email) || !user.EmailConfirmed)
            {
                return BadRequest(new StandardApiResponse()
                {
                    Status = Shared.Http.Status.Error,
                    Message = "Email must be set and verified to enable 2FA."
                });
            }
            
            if (!user.TwoFactorEnabled)
            {
                user.TwoFactorEnabled = true;
                var res = await _userManager.UpdateAsync(user);
                if (!res.Succeeded)
                {
                    return BadRequest(new StandardApiResponse
                    {
                        Status = Shared.Http.Status.Error,
                        Message = string.Join("; ", res.Errors.Select(e => e.Description))
                    });
                }
            }

            return Ok(new StandardApiResponse<TwoFactorResponseDto>
            {
                Body = new TwoFactorResponseDto
                {
                    Enabled = true,
                    Method = "Email" // TODO: Could perhaps be an enum? For 2fa methods I mean
                }
            });
        }

        private class TwoFactorResponseDto
        {
            public required bool Enabled { get; set; }
            public required string Method { get; set; }
        }

        [HttpPost("disable")]
        public async Task<IActionResult> Disable(CancellationToken ct)
        {
            var uid = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(uid))
            {
                return Unauthorized(new StandardApiResponse
                {
                    Status = Shared.Http.Status.Error,
                });
            }
            var user = await _userManager.FindByIdAsync(uid);
            if (user is null)
            {
                return Unauthorized(new StandardApiResponse()
                {
                    Status = Shared.Http.Status.Error,
                });
            }
            if (user.TwoFactorEnabled)
            {
                user.TwoFactorEnabled = false;
                var res = await _userManager.UpdateAsync(user);
                if (!res.Succeeded)
                {
                    return Unauthorized(new StandardApiResponse
                    {
                        Status = Shared.Http.Status.Error,
                        Message = string.Join("; ", res.Errors.Select(e => e.Description))
                    });
                }
            }

            return Ok(new StandardApiResponse<TwoFactorResponseDto>
            {
                Body = new TwoFactorResponseDto
                {
                    Enabled = false,
                    Method = "Email"
                }
            });
        }
    }
}