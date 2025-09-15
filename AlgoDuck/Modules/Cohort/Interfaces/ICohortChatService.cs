using AlgoDuck.Modules.Cohort.DTOs;

namespace AlgoDuck.Modules.Cohort.Interfaces;

public interface ICohortChatService
{
    Task<List<CohortChatDto>> GetMessagesAsync(Guid cohortId);
    Task<CohortChatDto> SaveMessageAsync(CohortChatDto dto);
}