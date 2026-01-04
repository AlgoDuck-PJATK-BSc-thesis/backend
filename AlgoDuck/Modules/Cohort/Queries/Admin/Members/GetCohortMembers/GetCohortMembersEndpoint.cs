using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;

[ApiController]
[Route("api/admin/cohorts/{cohortId:guid}/members")]
[Authorize(Roles = "admin")]
public sealed class GetCohortMembersEndpoint : ControllerBase
{
    private readonly IAdminGetCohortMembersHandler _handler;
    private readonly IValidator<GetCohortMembersRequestDto> _validator;

    public GetCohortMembersEndpoint(
        IAdminGetCohortMembersHandler handler,
        IValidator<GetCohortMembersRequestDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid cohortId, CancellationToken cancellationToken)
    {
        var dto = new GetCohortMembersRequestDto { CohortId = cohortId };
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var result = await _handler.HandleAsync(dto, cancellationToken);
        return Ok(result);
    }
}