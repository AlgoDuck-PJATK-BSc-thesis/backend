using AlgoDuck.DAL;
using AlgoDuck.Modules.Auth.DTOs;
using AlgoDuck.Modules.Auth.Interfaces;
using AlgoDuck.Modules.Auth.Models;
using AlgoDuck.Modules.User.Models;
using AlgoDuck.Shared.Exceptions;
using AlgoDuck.Modules.Auth.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AlgoDuck.Modules.Auth.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _dbContext;
    private readonly JwtSettings _jwtSettings;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ApplicationDbContext dbContext,
        IOptions<JwtSettings> options)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
        _jwtSettings = options.Value;
    }

    public async Task RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByNameAsync(dto.Username) != null)
            throw new UsernameAlreadyExistsException();

        if (await _userManager.FindByEmailAsync(dto.Email) is not null)
            throw new EmailAlreadyExistsException();

        var defaultRole = await _dbContext.UserRoles
            .FirstOrDefaultAsync(r => r.Name == "user", cancellationToken);

        if (defaultRole == null)
            throw new NotFoundException("Default role 'user' not found.");

        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            CohortId = null,
            Coins = 0,
            Experience = 0,
            AmountSolved = 0,
            UserRoleId = defaultRole.UserRoleId
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            throw new ValidationException(string.Join("; ", result.Errors.Select(e => e.Description)));
    }

    public async Task<(string AccessToken, string RefreshToken)> LoginAsync(LoginDto dto, HttpResponse response, CancellationToken cancellationToken)
    {
        var user = await _userManager.Users
            .Include(u => u.UserRole)
            .FirstOrDefaultAsync(u => u.UserName == dto.Username, cancellationToken);

        if (user == null)
            throw new UserNotFoundException();

        if (!await _userManager.CheckPasswordAsync(user, dto.Password))
            throw new UnauthorizedException("Invalid password.");

        var accessToken = _tokenService.CreateAccessToken(user);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var session = new Session
        {
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(7),
            UserId = user.Id,
            User = user
        };

        await _dbContext.Sessions.AddAsync(session, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        response.Cookies.Append("jwt", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes)
        });

        response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // TO DO : Set to true in production
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
        
        return (accessToken, refreshToken);
    }

    public async Task RefreshTokenAsync(RefreshDto dto, HttpResponse response, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .Include(s => s.User)
            .ThenInclude(u => u.UserRole)
            .FirstOrDefaultAsync(s => s.RefreshToken == dto.RefreshToken, cancellationToken);

        if (session == null || session.Revoked)
            throw new InvalidTokenException("Refresh token not found or revoked.");

        if (session.RefreshTokenExpiresAt < DateTime.UtcNow)
            throw new TokenExpiredException("Refresh token has expired.");

        var accessToken = _tokenService.CreateAccessToken(session.User);

        response.Cookies.Append("jwt", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = false, // TO DO : Set to true in production
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes)
        });
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = await _dbContext.Sessions
            .Where(s => s.UserId == userId && !s.Revoked)
            .ToListAsync(cancellationToken);
        
        if (sessions.Count == 0)
            throw new NotFoundException("No active sessions found for the user.");
        
        foreach (var session in sessions)
        {
            session.Revoked = true;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}