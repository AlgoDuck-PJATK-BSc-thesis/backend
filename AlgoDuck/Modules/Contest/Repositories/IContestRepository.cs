using ContestEntity = AlgoDuck.Modules.Contest.Models.Contest;
namespace AlgoDuck.Modules.Contest.Repositories;

public interface IContestRepository
{
    Task<IEnumerable<Modules.Contest.Models.Contest>> GetAllAsync();
    Task<Modules.Contest.Models.Contest?> GetByIdAsync(Guid id);
    Task AddAsync(Modules.Contest.Models.Contest contest);
    Task DeleteAsync(Modules.Contest.Models.Contest contest);
    Task SaveChangesAsync();
}