using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Shared.Repositories;

public interface ISharedTestCaseRepository{
    public Task<Result<bool, ErrorObject<string>>> ValidateAllTestCasesPassedAsync(ValidationRequestDto validationRequest, CancellationToken cancellationToken = default);
}

public class SharedTestCaseRepository : ISharedTestCaseRepository
{
    private readonly ApplicationQueryDbContext _dbContext;

    public SharedTestCaseRepository(ApplicationQueryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<bool, ErrorObject<string>>> ValidateAllTestCasesPassedAsync(ValidationRequestDto validationRequest,
        CancellationToken cancellationToken = default)
    {
        var testCases = await _dbContext.TestCases.Where(t => t.ProblemProblemId == validationRequest.ProblemId)
            .ToListAsync(cancellationToken: cancellationToken);
        
        if (testCases.Count == 0)
            return Result<bool, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"test cases not found for problem id: {validationRequest.ProblemId}"));
            
        return Result<bool, ErrorObject<string>>.Ok(testCases.All(tc => validationRequest.TestingResults.TryGetValue(tc.TestCaseId, out var result) && result));
    }
    
}

public class ValidationRequestDto
{
    public required Guid ProblemId { get; init; }
    public required Dictionary<Guid, bool> TestingResults { get; init; }
}