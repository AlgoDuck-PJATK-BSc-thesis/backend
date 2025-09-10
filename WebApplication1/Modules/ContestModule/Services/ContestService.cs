using Microsoft.EntityFrameworkCore;
using WebApplication1.DAL;
using WebApplication1.Modules.Contest.DTOs;
using ContestEntity = WebApplication1.Modules.ContestModule.Models.Contest;
using WebApplication1.Modules.ContestModule.Models;
using WebApplication1.Modules.ProblemModule.Models;



namespace WebApplication1.Modules.Contest.Services;

public class ContestService : IContestService
{
    private readonly ApplicationDbContext _db;

    public ContestService(ApplicationDbContext db)
    {
        _db = db;
    }
    
    public async Task<Guid> CreateContestAsync(CreateContestDto dto)
    {
        var item = await _db.Items.FindAsync(dto.ItemId);
        if (item == null)
        {
            throw new Exception("Item not found");
        }
        var problems = await _db.Problems
            .Where(p => dto.ProblemIds.Contains(p.ProblemId))
            .ToListAsync();
            
        var contest = new ContestEntity
        {
            ContestName = dto.ContestName,
            ContestDescription = dto.ContestDescription,
            ContestStartDate = dto.ContestStartDateTime,
            ContestEndDate = dto.ContestEndDateTime,
            ItemId = dto.ItemId,
            Item = item,
            ContestProblems = new List<ContestProblem>()
            
        };

        foreach (var problem in problems)
        {
            contest.ContestProblems.Add(new ContestProblem
            {
                Contest = contest,
                Problem = problem,
                ProblemId = problem.ProblemId
            });
        }
        
        _db.Contests.Add(contest);
        await _db.SaveChangesAsync();
        return contest.ContestId;
    }

    public async Task<ContestEntity?> GetContestByIdAsync(Guid id)
    {
        return await _db.Contests
            .Include(c => c.ContestProblems)
            .ThenInclude(cp => cp.Problem)
            .FirstOrDefaultAsync(c => c.ContestId == id);
    }

    public async Task<bool> DeleteContestAsync(Guid id)
    {
        var contest = await _db.Contests
            .Include(c => c.ContestProblems)
            .FirstOrDefaultAsync(c => c.ContestId == id);

        if (contest == null)
        {
            return false;
        }
        
        _db.ContestProblems.RemoveRange(contest.ContestProblems);
        _db.Contests.Remove(contest);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task AddProblemToContest(Guid contestId, Guid problemId)
    {
        var contest = await _db.Contests.FindAsync(contestId);
        if (contest == null)
            throw new Exception("Contest not found");

        var problem = await _db.Problems.FindAsync(problemId);
        if (problem == null)
            throw new Exception("Problem not found");

        var alreadyExists = await _db.ContestProblems
            .AnyAsync(cp => contest.ContestId == contestId && cp.ProblemId == problemId);
        if (alreadyExists)
            throw new Exception("This problem is already assingend to the contest");

        var contestProblem = new ContestProblem
        {
            ContestId = contestId,
            Contest = contest,
            ProblemId = problemId,
            Problem = problem
        };

        _db.ContestProblems.Add(contestProblem);
        await _db.SaveChangesAsync();
    }

    public async Task RemoveProblemFromContest(Guid contestId, Guid problemId)
    {
        var contestProblem = await _db.ContestProblems
            .FirstOrDefaultAsync(cp => cp.ContestId == contestId && cp.ProblemId == problemId);

        if (contestProblem == null)
        {
            throw new Exception("Problem not found in this contest");
        }

        _db.ContestProblems.Remove(contestProblem);
        await _db.SaveChangesAsync();
    }
}