using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;
using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Cohort.Controllers;

[ApiController]
[Route("api/cohorts")]
[Authorize]
public class CohortController : ControllerBase
{
    private readonly ICohortService _service;

    public CohortController(ICohortService service)
    {
        _service = service;
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Get(Guid id)
    {
        var cohort = await _service.GetByIdAsync(id);
        return cohort is null
            ? NotFound(ApiResponse.Fail("Cohort not found.", "not_found"))
            : Ok(ApiResponse.Success(cohort));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCohortDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

        var result = await _service.CreateAsync(dto, Guid.Parse(userId));
        return Ok(ApiResponse.Success(result));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

        var result = await _service.GetMineAsync(Guid.Parse(userId));
        return result is null
            ? NotFound(ApiResponse.Fail("Cohort not found.", "not_found"))
            : Ok(ApiResponse.Success(result));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCohortDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto);
        return ok
            ? Ok(ApiResponse.Success(new { message = "Cohort updated." }))
            : NotFound(ApiResponse.Fail("Cohort not found.", "not_found"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok
            ? Ok(ApiResponse.Success(new { message = "Cohort deleted." }))
            : NotFound(ApiResponse.Fail("Cohort not found.", "not_found"));
    }

    [HttpPost("{id:guid}/users/{userId:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddUser(Guid id, Guid userId)
    {
        var ok = await _service.AddUserAsync(id, userId);
        return ok
            ? Ok(ApiResponse.Success(new { message = "User added to cohort." }))
            : NotFound(ApiResponse.Fail("Cohort or user not found.", "not_found"));
    }

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId)
    {
        var ok = await _service.RemoveUserAsync(id, userId);
        return ok
            ? Ok(ApiResponse.Success(new { message = "User removed from cohort." }))
            : NotFound(ApiResponse.Fail("Cohort or user not found.", "not_found"));
    }

    [HttpGet("{id:guid}/users")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetUsers(Guid id)
        => Ok(ApiResponse.Success(await _service.GetUsersAsync(id)));
    
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> GetDetails(Guid id, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
            return Unauthorized(ApiResponse.Fail("Unauthorized", "unauthorized"));

        var requesterId = Guid.Parse(userIdStr);
        var isAdmin = User.IsInRole("admin");

        var details = await _service.GetDetailsAsync(id, requesterId, isAdmin, ct);
        return details is null
            ? NotFound(ApiResponse.Fail("Cohort not found.", "not_found"))
            : Ok(ApiResponse.Success(details));
    }
} 