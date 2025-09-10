using ContestEntity = WebApplication1.Modules.ContestModule.Models.Contest;
namespace WebApplication1.Modules.Contest.Repositories;

public interface IContestRepository
{
    Task<IEnumerable<ContestEntity>> GetAllAsync();
    Task<ContestEntity?> GetByIdAsync(Guid id);
    Task AddAsync(ContestEntity contest);
    Task DeleteAsync(ContestEntity contest);
    Task SaveChangesAsync();
}