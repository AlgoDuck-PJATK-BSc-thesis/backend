using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;
using AlgoDuck.Modules.Contest.Models;
using ContestEntity = AlgoDuck.Modules.Contest.Models.Contest;

namespace AlgoDuck.Modules.Contest.Repositories;

public class ContestRepository : IContestRepository
{
    private readonly ApplicationDbContext _db;
    public ContestRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<Modules.Contest.Models.Contest>> GetAllAsync()
    {
        return await _db.Contests
            .Include(c => c.ContestProblems)
            .ThenInclude(cp => cp.Problem)
            .ToListAsync();
    }
       

    public async Task<Modules.Contest.Models.Contest?> GetByIdAsync(Guid id)
    {
        return await _db.Contests
            .Include(c => c.ContestProblems)
            .ThenInclude(cp => cp.Problem)
            .FirstOrDefaultAsync(c => c.ContestId == id);
    }

    public async Task AddAsync(Modules.Contest.Models.Contest contest)
    {
        await _db.Contests.AddAsync(contest);
    }

    public async Task DeleteAsync(Modules.Contest.Models.Contest contest)
    {
        _db.ContestProblems.RemoveRange(contest.ContestProblems);
        _db.Contests.Remove(contest);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
    

    
}