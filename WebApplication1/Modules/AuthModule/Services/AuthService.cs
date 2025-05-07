using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Modules.AuthModule.DTOs;
using WebApplication1.Modules.AuthModule.Models;
using WebApplication1.Modules.AuthModule.Interfaces;
using WebApplication1.Modules.UserModule.Models;
using WebApplication1.DAL;
using WebApplication1.Shared.Exceptions;


namespace WebApplication1.Modules.AuthModule.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ApplicationDbContext _dbContext;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        ITokenService tokenService,
        ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _dbContext = dbContext;
    }

    public async Task RegisterAsync(RegisterDto dto, CancellationToken cancellationToken)
    {
        if (await _userManager.FindByNameAsync(dto.Username) != null)
            throw new UsernameAlreadyExistsException();

        if (await _userManager.Users.AnyAsync(u => u.Email == dto.Email, cancellationToken))
            throw new EmailAlreadyExistsException();
        
        var user = new ApplicationUser
        {
            UserName = dto.Username,
            Email = dto.Email,
            CohortId = dto.CohortId,
            Coins = 0,
            Experience = 0,
            AmountSolved = 0
        };

        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            throw new ValidationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    public async Task LoginAsync(LoginDto dto, HttpResponse response, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(dto.Username);
        
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
            Expires = DateTimeOffset.UtcNow.AddMinutes(120)
        });

        response.Cookies.Append("refresh_token", refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        });
    }

    public async Task RefreshTokenAsync(RefreshDto dto, HttpResponse response, CancellationToken cancellationToken)
    {
        var session = await _dbContext.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.RefreshToken == dto.RefreshToken, cancellationToken);

        if (session == null)
        {
            throw new InvalidTokenException("Refresh token not found.");
        }

        if (session.RefreshTokenExpiresAt < DateTime.UtcNow)
        {
            throw new TokenExpiredException("Refresh token has expired.");
        }

        var accessToken = _tokenService.CreateAccessToken(session.User);

        response.Cookies.Append("jwt", accessToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None, // TODO zmienić na Strict
            Expires = DateTimeOffset.UtcNow.AddMinutes(120)
        });
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken)
    {
        var sessions = _dbContext.Sessions.Where(s => s.UserId == userId);
        
        if (!await sessions.AnyAsync(cancellationToken))
            throw new NotFoundException("No sessions found for user.");
        
        _dbContext.Sessions.RemoveRange(sessions);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
