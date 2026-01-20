using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Item.Queries.GetUserPreviewData;

[Authorize(Roles = "admin")]
[Route("api/user/item")]
[ApiController]
public class UserPreviewDataController
{
    private readonly IGetUserPreviewService _getUserPreviewService;

    public UserPreviewDataController(IGetUserPreviewService getUserPreviewService)
    {
        _getUserPreviewService = getUserPreviewService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUserPreviewAsync([FromQuery] Guid userId, CancellationToken cancellationToken)
    {
        return await _getUserPreviewService.GetUserPreviewAsync(userId, cancellationToken).ToActionResultAsync();
    }
}
