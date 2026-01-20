using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Cohorts.SearchCohorts;

[ApiController]
[Route("api/admin/cohorts/search")]
[Authorize(Roles = "admin")]
public sealed class SearchCohortsEndpoint : ControllerBase
{
    private readonly ISearchCohortsHandler _handler;
    private readonly IValidator<SearchCohortsDto> _validator;

    public SearchCohortsEndpoint(ISearchCohortsHandler handler, IValidator<SearchCohortsDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] SearchCohortsDto request, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _handler.HandleAsync(request, cancellationToken);
        return Ok(result);
    }
}