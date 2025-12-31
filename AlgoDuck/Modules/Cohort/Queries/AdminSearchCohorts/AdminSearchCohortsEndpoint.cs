using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.AdminSearchCohorts;

[ApiController]
[Route("api/admin/cohorts/search")]
[Authorize(Roles = "admin")]
public sealed class AdminSearchCohortsEndpoint : ControllerBase
{
    private readonly IAdminSearchCohortsHandler _handler;
    private readonly IValidator<AdminSearchCohortsDto> _validator;

    public AdminSearchCohortsEndpoint(IAdminSearchCohortsHandler handler, IValidator<AdminSearchCohortsDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AdminSearchCohortsDto request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _handler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}