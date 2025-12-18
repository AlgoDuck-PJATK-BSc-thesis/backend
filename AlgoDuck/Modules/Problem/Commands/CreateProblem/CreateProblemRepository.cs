using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.CreateProblem;

public interface ICreateProblemRepository
{
    public Task<Result<CreateProblemResultDto, ErrorObject<string>>> CreateProblemAsync(CreateProblemDto problemDto, CancellationToken cancellationToken = default);
}

public class CreateProblemRepository(
    ApplicationCommandDbContext commandDbContext
) : ICreateProblemRepository
{
    public Task<Result<CreateProblemResultDto, ErrorObject<string>>> CreateProblemAsync(CreateProblemDto problemDto, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}