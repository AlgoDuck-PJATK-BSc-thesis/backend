using ContestEntity = AlgoDuck.Models.Contest.Contest;
namespace AlgoDuck.Modules.Contest.Repositories;

public interface IContestRepository
{
    Task<IEnumerable<ContestEntity>> GetAllAsync();
    Task<ContestEntity?> GetByIdAsync(Guid id);
    Task AddAsync(ContestEntity contest);
    Task DeleteAsync(ContestEntity contest);
    Task SaveChangesAsync();
}