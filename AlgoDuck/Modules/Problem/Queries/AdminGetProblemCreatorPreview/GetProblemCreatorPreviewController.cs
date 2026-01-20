using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Queries.AdminGetProblemCreatorPreview;

[Route("api/problem/creator/preview")]
[ApiController]
[Authorize(Roles = "admin")]
public class GetProblemCreatorPreviewController : ControllerBase
{
    private readonly IGetProblemCreatorPreviewService _getProblemCreatorPreviewService;

    public GetProblemCreatorPreviewController(IGetProblemCreatorPreviewService getProblemCreatorPreviewService)
    {
        _getProblemCreatorPreviewService = getProblemCreatorPreviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetProblemCreatorPreviewAsync([FromQuery] Guid userId,
        CancellationToken cancellationToken)
    {
        return await _getProblemCreatorPreviewService.GetProblemCreatorAsync(userId, cancellationToken).ToActionResultAsync();
    }
}


public class ProblemCreator
{
    public required Guid Id { get; init; }
    public required string Username { get; init; }
    public required string Email { get; init; }
    public required Guid SelectedAvatar { get; init; }
    public required int ProblemCreatedCount { get; init; }
}