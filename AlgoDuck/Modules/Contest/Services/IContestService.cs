using AlgoDuck.Modules.Contest.DTOs;
using ContestEntity = AlgoDuck.Modules.Contest.Models.Contest;
namespace AlgoDuck.Modules.Contest.Services;

public interface IContestService
{
    Task<Guid> CreateContestAsync(CreateContestDto dto);
    Task<bool> DeleteContestAsync(Guid id);
    Task AddProblemToContest(Guid contestId, Guid problemId);
    Task RemoveProblemFromContest(Guid contestId, Guid problemId);
    Task<Modules.Contest.Models.Contest?> GetContestByIdAsync(Guid id);
}