using WebApplication1.Modules.Contest.DTOs;
using ContestEntity = WebApplication1.Modules.ContestModule.Models.Contest;
namespace WebApplication1.Modules.Contest.Services;

public interface IContestService
{
    Task<Guid> CreateContestAsync(CreateContestDto dto);
    Task<bool> DeleteContestAsync(Guid id);
    Task AddProblemToContest(Guid contestId, Guid problemId);
    Task RemoveProblemFromContest(Guid contestId, Guid problemId);
    Task<ContestEntity?> GetContestByIdAsync(Guid id);
}