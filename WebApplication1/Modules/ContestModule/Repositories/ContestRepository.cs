using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Modules.ContestModule.Models;
using ContestEntity = WebApplication1.Modules.ContestModule.Models.Contest;

namespace WebApplication1.Modules.Contest.Repositories;

public class ContestRepository : IContestRepository
{
    private readonly ApplicationDbContext _db;
    public ContestRepository(ApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<ContestEntity>> GetAllAsync()
    {
        return await _db.Contests
            .Include(c => c.ContestProblems)
            .ThenInclude(cp => cp.Problem)
            .ToListAsync();
    }
       

    public async Task<ContestEntity?> GetByIdAsync(Guid id)
    {
        return await _db.Contests
            .Include(c => c.ContestProblems)
            .ThenInclude(cp => cp.Problem)
            .FirstOrDefaultAsync(c => c.ContestId == id);
    }

    public async Task AddAsync(ContestEntity contest)
    {
        await _db.Contests.AddAsync(contest);
    }

    public async Task DeleteAsync(ContestEntity contest)
    {
        _db.ContestProblems.RemoveRange(contest.ContestProblems);
        _db.Contests.Remove(contest);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
    

    
}