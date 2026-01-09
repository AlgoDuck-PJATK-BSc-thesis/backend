using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.DeleteProblem;

public interface IDeleteProblemService
{
    public Task<Result<DeleteProblemResultDto, ErrorObject<string>>> DeleteProblemAsync(Guid problemId, CancellationToken cancellationToken = default);
}

public class DeleteProblemService : IDeleteProblemService
{
    private readonly IDeleteProblemRepository _deleteProblemRepository;

    public DeleteProblemService(IDeleteProblemRepository deleteProblemRepository)
    {
        _deleteProblemRepository = deleteProblemRepository;
    }

    public async Task<Result<DeleteProblemResultDto, ErrorObject<string>>> DeleteProblemAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        return await _deleteProblemRepository
            .DeleteProblemRdbAsync(problemId, cancellationToken)
            .BindAsync(async id =>
            {
                var result = await _deleteProblemRepository.DeleteProblemS3Async(id.ProblemId, cancellationToken);
                if (result.IsErr)
                    return Result<DeleteProblemResultDto, ErrorObject<string>>.Err(result.AsErr!);
                return Result<DeleteProblemResultDto, ErrorObject<string>>.Ok(id);
            });
    }
}