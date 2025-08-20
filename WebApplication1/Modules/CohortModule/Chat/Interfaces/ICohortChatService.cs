using WebApplication1.Modules.CohortModule.Chat.DTOs;

namespace WebApplication1.Modules.CohortModule.Chat.Interfaces;

public interface ICohortChatService
{
    Task<List<CohortChatDto>> GetMessagesAsync(Guid cohortId);
    Task<CohortChatDto> SaveMessageAsync(CohortChatDto dto);
}