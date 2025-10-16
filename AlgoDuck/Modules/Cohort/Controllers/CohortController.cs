using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using AlgoDuck.Modules.Cohort.DTOs;
using AlgoDuck.Modules.Cohort.Interfaces;

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
        return cohort is null ? NotFound() : Ok(cohort);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCohortDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _service.CreateAsync(dto, Guid.Parse(userId));
        return Ok(result);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _service.GetMineAsync(Guid.Parse(userId));
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCohortDto dto) =>
        await _service.UpdateAsync(id, dto) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id) =>
        await _service.DeleteAsync(id) ? NoContent() : NotFound();

    [HttpPost("{id:guid}/users/{userId:guid}")]
    public async Task<IActionResult> AddUser(Guid id, Guid userId) =>
        await _service.AddUserAsync(id, userId) ? NoContent() : NotFound();

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId) =>
        await _service.RemoveUserAsync(id, userId) ? NoContent() : NotFound();

    [HttpGet("{id:guid}/users")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetUsers(Guid id) =>
        Ok(await _service.GetUsersAsync(id));
} 