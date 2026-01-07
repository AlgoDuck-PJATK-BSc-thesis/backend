using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.GetCohorts;


[ApiController]
[Route("api/admin/cohorts")]
[Authorize(Roles = "admin")]
public sealed class AdminGetCohortsEndpoint : ControllerBase
{
    private readonly IAdminGetCohortsHandler _handler;
    private readonly IValidator<AdminGetCohortsDto> _validator;

    public AdminGetCohortsEndpoint(IAdminGetCohortsHandler handler, IValidator<AdminGetCohortsDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] AdminGetCohortsDto request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _handler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}