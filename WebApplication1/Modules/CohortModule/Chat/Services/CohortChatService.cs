using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication1.DAL;
using WebApplication1.Modules.CohortModule.Chat.DTOs;
using WebApplication1.Modules.CohortModule.Chat.Interfaces;
using WebApplication1.Modules.CohortModule.Models;
using WebApplication1.Shared.Exceptions;

namespace WebApplication1.Modules.CohortModule.Chat.Services;

public class CohortChatService : ICohortChatService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CohortChatService(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<CohortChatDto>> GetMessagesAsync(Guid cohortId)
    {
        var userId = GetCurrentUserId();

        var belongsToCohort = await _dbContext.Users
            .AnyAsync(u => u.Id == userId && u.CohortId == cohortId);

        if (!belongsToCohort)
            throw new ForbiddenException("You are not a member of this cohort.");

        return await _dbContext.Messages
            .Include(m => m.User)
            .Where(m => m.CohortId == cohortId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new CohortChatDto
            {
                CohortId = m.CohortId,
                UserId = m.UserId,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                Username = m.User!.UserName,
                UserProfilePicture = m.User.ProfilePicture
            })
            .ToListAsync();
    }

    public async Task<CohortChatDto> SaveMessageAsync(CohortChatDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId != dto.UserId)
            throw new ForbiddenException("You can only send messages as yourself.");

        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new Exception("User not found");

        if (user.CohortId != dto.CohortId)
            throw new ForbiddenException("You cannot send messages to this cohort.");

        var message = new Message
        {
            Content = dto.Content,
            CohortId = dto.CohortId,
            UserId = dto.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Messages.Add(message);
        await _dbContext.SaveChangesAsync();

        return new CohortChatDto
        {
            CohortId = message.CohortId,
            UserId = message.UserId,
            Content = message.Content,
            CreatedAt = message.CreatedAt,
            Username = user.UserName,
            UserProfilePicture = user.ProfilePicture
        };
    }

    private Guid GetCurrentUserId()
    {
        var userId = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guid))
            throw new UnauthorizedException();

        return guid;
    }
}