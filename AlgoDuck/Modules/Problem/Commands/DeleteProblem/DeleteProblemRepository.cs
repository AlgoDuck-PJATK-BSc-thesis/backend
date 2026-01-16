using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using AlgoDuck.Shared.S3;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.DeleteProblem;

public interface IDeleteProblemRepository
{
    public Task<Result<DeleteProblemResultDto, ErrorObject<string>>> DeleteProblemRdbAsync(Guid problemId, CancellationToken cancellationToken = default);
    public Task<Result<ICollection<string>, ErrorObject<string>>> DeleteProblemS3Async(Guid problemId, CancellationToken cancellationToken = default);
}

public class DeleteProblemRepository : IDeleteProblemRepository
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IAwsS3Client _awsS3Client;

    public DeleteProblemRepository(ApplicationCommandDbContext dbContext, IAwsS3Client awsS3Client)
    {
        _dbContext = dbContext;
        _awsS3Client = awsS3Client;
    }

    public async Task<Result<DeleteProblemResultDto, ErrorObject<string>>> DeleteProblemRdbAsync(Guid problemId, CancellationToken cancellationToken = default)
    {
        var rowsAffected = await _dbContext.Problems.Where(p => p.ProblemId == problemId)
            .ExecuteDeleteAsync(cancellationToken: cancellationToken);
        if (rowsAffected == 0)
            return Result<DeleteProblemResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"problem with id: {problemId} not found"));
        return Result<DeleteProblemResultDto, ErrorObject<string>>.Ok(new DeleteProblemResultDto
        {
            ProblemId = problemId
        });
    }

    public async Task<Result<ICollection<string>, ErrorObject<string>>> DeleteProblemS3Async(Guid problemId, CancellationToken cancellationToken = default)
    {
        return await _awsS3Client.DeleteAllByPrefixAsync($"problems/{problemId}", cancellationToken: cancellationToken);
    }
}