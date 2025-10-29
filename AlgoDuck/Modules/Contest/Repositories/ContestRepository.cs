using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;
using ContestEntity = AlgoDuck.Models.Contest;

namespace AlgoDuck.Modules.Contest.Repositories;

public class ContestRepository : IContestRepository
{
    private readonly ApplicationDbContext _db;
    public ContestRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<ContestEntity.Contest>> GetAllAsync()
    {
        return await _db.Contests
            .Include(c => c.ContestProblems)
            .ThenInclude(cp => cp.Problem)
            .ToListAsync();
    }
       

    public async Task<ContestEntity.Contest?> GetByIdAsync(Guid id)
    {
        return await _db.Contests
            .Include(c => c.ContestProblems)
            .ThenInclude(cp => cp.Problem)
            .FirstOrDefaultAsync(c => c.ContestId == id);
    }

    public async Task AddAsync(ContestEntity.Contest contest)
    {
        await _db.Contests.AddAsync(contest);
    }

    public async Task DeleteAsync(ContestEntity.Contest contest)
    {
        _db.ContestProblems.RemoveRange(contest.ContestProblems);
        _db.Contests.Remove(contest);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
    
}