using AlgoDuck.Modules.Contest.DTOs;
using ContestEntity = AlgoDuck.Models.Contest.Contest;
namespace AlgoDuck.Modules.Contest.Services;

public interface IContestService
{
    Task<Guid> CreateContestAsync(CreateContestDto dto);
    Task<bool> DeleteContestAsync(Guid id);
    Task AddProblemToContest(Guid contestId, Guid problemId);
    Task RemoveProblemFromContest(Guid contestId, Guid problemId);
    Task<ContestEntity?> GetContestByIdAsync(Guid id);
}