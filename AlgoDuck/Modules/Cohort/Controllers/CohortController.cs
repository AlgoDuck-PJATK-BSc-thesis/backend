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
            ? NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Cohort not found"
            })
            : Ok(new StandardApiResponse<CohortDto>
            {
                Body = cohort
            });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCohortDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
            });
        }
        
        return Ok(new StandardApiResponse<CohortDto>
        {
            Body = await _service.CreateAsync(dto, Guid.Parse(userId))
        });
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error
            });
        }
        
        var result = await _service.GetMineAsync(Guid.Parse(userId));
        return result is null
            ? NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Cohort not found"
            })
            : Ok(new StandardApiResponse<CohortDto>
            {
                Body = result
            });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateCohortDto dto)
    {
        var ok = await _service.UpdateAsync(id, dto);
        return ok
            ? Ok(new StandardApiResponse
            {
                Message = "Cohort updated"
            })
            : NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Cohort not found"
            });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var ok = await _service.DeleteAsync(id);
        return ok
            ? Ok(new StandardApiResponse
            {
                Message = "Cohort Archived"
            })
            : NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Cohort not found"
            });
    }

    [HttpPost("{id:guid}/users/{userId:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> AddUser(Guid id, Guid userId)
    {
        var ok = await _service.AddUserAsync(id, userId);
        return ok
            ? Ok(new StandardApiResponse
            {
                Message = "User added to cohort"
            })
            : NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Cohort or user not found"
            });
    }

    [HttpDelete("{id:guid}/users/{userId:guid}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> RemoveUser(Guid id, Guid userId)
    {
        var ok = await _service.RemoveUserAsync(id, userId);
        return ok
            ? Ok(new StandardApiResponse
            {
                Message = "User removed from cohort"
            })
            : NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Cohort or user not found"
            });
    }

    [HttpGet("{id:guid}/users")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetUsers(Guid id) => Ok(new StandardApiResponse<List<UserProfileDto>>
    {
        Body = await _service.GetUsersAsync(id)
    });
    
    [HttpGet("{id:guid}/details")]
    public async Task<IActionResult> GetDetails(Guid id, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr))
        {
            return Unauthorized(new StandardApiResponse
            {
                Status = Status.Error,
            });
        }
        
        var requesterId = Guid.Parse(userIdStr);
        var isAdmin = User.IsInRole("admin");

        var details = await _service.GetDetailsAsync(id, requesterId, isAdmin, ct);
        return details is null
            ? NotFound(new StandardApiResponse
            {
                Status = Status.Error,
                Message = "Cohort not found"
            })
            : Ok(new StandardApiResponse<CohortDetailsDto>
            {
                Body = details
            });
    }
} 