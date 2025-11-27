using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;
using AlgoDuck.Shared.Exceptions;

namespace AlgoDuck.Modules.Cohort.Services;

public class CohortChatService : ICohortChatService
{
    private readonly ApplicationCommandDbContext _commandDbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CohortChatService(ApplicationCommandDbContext commandDbContext, IHttpContextAccessor httpContextAccessor)
    {
        _commandDbContext = commandDbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<List<CohortChatDto>> GetMessagesAsync(Guid cohortId)
    {
        var userId = GetCurrentUserId();

        var belongsToCohort = await _commandDbContext.ApplicationUsers
            .AnyAsync(u => u.Id == userId && u.CohortId == cohortId);

        if (!belongsToCohort)
            throw new ForbiddenException("You are not a member of this cohort.");

        return await _commandDbContext.Messages
            .Include(m => m.User)
            .Where(m => m.CohortId == cohortId)
            .OrderBy(m => m.CreatedAt)
            .Select(m => new CohortChatDto
            {
                CohortId = m.CohortId,
                UserId = m.UserId,
                Content = m.Message1,
                CreatedAt = m.CreatedAt,
                Username = m.User!.UserName,
            })
            .ToListAsync();
    }

    public async Task<CohortChatDto> SaveMessageAsync(CohortChatDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId != dto.UserId)
            throw new ForbiddenException("You can only send messages as yourself.");

        var user = await _commandDbContext.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == userId)
                   ?? throw new Exception("User not found");

        if (user.CohortId != dto.CohortId)
            throw new ForbiddenException("You cannot send messages to this cohort.");

        var message = new Message
        {
            Message1 = dto.Content,
            CohortId = dto.CohortId,
            UserId = dto.UserId,
            CreatedAt = DateTime.UtcNow
        };

        _commandDbContext.Messages.Add(message);
        await _commandDbContext.SaveChangesAsync();

        return new CohortChatDto
        {
            CohortId = message.CohortId,
            UserId = message.UserId,
            Content = message.Message1,
            CreatedAt = message.CreatedAt,
            Username = user.UserName,
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