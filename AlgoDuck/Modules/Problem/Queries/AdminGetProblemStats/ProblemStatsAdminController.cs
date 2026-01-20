using AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin;
using AlgoDuck.Modules.Problem.Queries.GetProblemStatsAdmin.Types;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetProblemStats;

[ApiController]
[Route("api/admin/problem/stats")]
[Authorize(Roles = "admin")]
public class ProblemStatsAdminController
{
    private readonly IProblemDetailsAdminService _service;

    public ProblemStatsAdminController(IProblemDetailsAdminService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetProblemStatsAsync(
        [FromQuery] Guid problemId,
        [FromQuery] int runtimeBucketSize,
        [FromQuery] int memBucketSize,
        [FromQuery] int recentSubmissionCount,
        CancellationToken cancellationToken)
    {
        return await _service.GetProblemStatsAsync(new PerformanceRequest()
        {
            MemoryBucketSize = memBucketSize,
            ProblemId = problemId,
            RuntimeBucketSize = runtimeBucketSize
        }, new RecentActivityRequest
        {
            ProblemId = problemId,
            RecentCount = recentSubmissionCount,
        }, cancellationToken).ToActionResultAsync();
    }
}

