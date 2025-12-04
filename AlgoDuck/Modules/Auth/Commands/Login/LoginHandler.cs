using System.Security.Cryptography;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Jwt;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using AlgoDuck.Shared.Utilities;

namespace AlgoDuck.Modules.Auth.Commands.Login;

public sealed class LoginHandler : ILoginHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationCommandDbContext _commandDbContext;
    private readonly JwtSettings _jwtSettings;

    public LoginHandler(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ApplicationCommandDbContext commandDbContext,
        IOptions<JwtSettings> jwtOptions)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _commandDbContext = commandDbContext;
        _jwtSettings = jwtOptions.Value;
    }

    public async Task<AuthResponse> HandleAsync(LoginDto dto, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .FirstOrDefaultAsync(u => u.UserName == dto.Username || u.Email == dto.Username, cancellationToken);

        if (user == null)
            throw new UserNotFoundException();

        if (await _userManager.IsLockedOutAsync(user))
            throw new ForbiddenException("Account is locked. Try again later.");

        var passwordOk = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordOk)
        {
            await _userManager.AccessFailedAsync(user);

            if (await _userManager.IsLockedOutAsync(user))
                throw new ForbiddenException("Account is locked. Try again later.");

            throw new UnauthorizedException("Invalid password.");
        }

        await _userManager.ResetAccessFailedCountAsync(user);

        var accessToken = await _tokenService.CreateAccessTokenAsync(user);

        var rawRefresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var saltBytes = HashingHelper.GenerateSalt();
        var hashB64 = HashingHelper.HashPassword(rawRefresh, saltBytes);
        var saltB64 = Convert.ToBase64String(saltBytes);

        var prefixLength = Math.Min(rawRefresh.Length, 32);
        var refreshPrefix = rawRefresh.Substring(0, prefixLength);

        var nowUtc = DateTime.UtcNow;
        var refreshExpiresUtc = nowUtc.AddDays(_jwtSettings.RefreshDays);

        var session = new Session
        {
            SessionId = Guid.NewGuid(),
            RefreshTokenHash = hashB64,
            RefreshTokenSalt = saltB64,
            RefreshTokenPrefix = refreshPrefix,
            CreatedAtUtc = nowUtc,
            ExpiresAtUtc = refreshExpiresUtc,
            UserId = user.Id,
            User = user
        };

        await _commandDbContext.Sessions.AddAsync(session, cancellationToken);
        await _commandDbContext.SaveChangesAsync(cancellationToken);

        var accessExpiresAt = nowUtc.AddMinutes(_jwtSettings.DurationInMinutes);

        var response = new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = rawRefresh,
            CsrfToken = string.Empty,
            AccessTokenExpiresAt = accessExpiresAt,
            RefreshTokenExpiresAt = refreshExpiresUtc,
            SessionId = session.SessionId,
            UserId = user.Id
        };

        return response;
    }
}