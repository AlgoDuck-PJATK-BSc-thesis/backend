using AlgoDuck.Modules.Cohort.DTOs;

namespace AlgoDuck.Modules.Cohort.Interfaces;

public interface ICohortService
{
    Task<List<CohortDto>> GetAllAsync();
    Task<CohortDto?> GetByIdAsync(Guid id);
    Task<CohortDto> CreateAsync(CreateCohortDto dto, Guid currentUserId);
    Task<CohortDto?> GetMineAsync(Guid currentUserId);    
    Task<bool> UpdateAsync(Guid id, UpdateCohortDto dto);
    Task<bool> DeleteAsync(Guid id);
    
    Task<bool> AddUserAsync(Guid cohortId, Guid userId);
    Task<bool> RemoveUserAsync(Guid cohortId, Guid userId);
    Task<List<UserProfileDto>> GetUsersAsync(Guid cohortId);
}