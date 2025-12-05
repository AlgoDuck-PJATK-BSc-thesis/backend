using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Jwt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Services;

public sealed class DevelopmentAuthService : AuthService
{
    public DevelopmentAuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ApplicationCommandDbContext commandDbContext,
        IOptions<JwtSettings> options,
        IWebHostEnvironment env,
        IHttpContextAccessor http,
        ILogger<AuthService> logger)
        : base(userManager, tokenService, commandDbContext, options, env, http, logger)
    {
    }
}