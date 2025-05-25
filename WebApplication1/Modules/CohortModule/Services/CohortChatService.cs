using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Modules.CohortModule.DTOs;
using WebApplication1.Modules.CohortModule.Interfaces;
using WebApplication1.Modules.CohortModule.Models;

namespace WebApplication1.Modules.CohortModule.Services;

public class CohortChatService : ICohortChatService
{
    private readonly ApplicationDbContext _dbContext;

    public CohortChatService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<CohortChatDto>> GetMessagesAsync(Guid cohortId)
    {
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
                Username = m.User.UserName,
                UserProfilePicture = m.User.ProfilePicture
            })
            .ToListAsync();
    }

    public async Task<CohortChatDto> SaveMessageAsync(CohortChatDto dto)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId)
                   ?? throw new Exception("User not found");

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
}