using WebApplication1.Modules.CohortModule.DTOs;

namespace WebApplication1.Modules.CohortModule.Interfaces;

public interface ICohortChatService
{
    Task<List<CohortChatDto>> GetMessagesAsync(Guid cohortId);
    Task<CohortChatDto> SaveMessageAsync(CohortChatDto dto);
}