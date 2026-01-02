using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.Queries.AdminGetCohortMembers;

[ApiController]
[Route("api/admin/cohorts/{cohortId:guid}/members")]
[Authorize(Roles = "admin")]
public sealed class AdminGetCohortMembersEndpoint : ControllerBase
{
    private readonly IAdminGetCohortMembersHandler _handler;
    private readonly IValidator<AdminGetCohortMembersRequestDto> _validator;

    public AdminGetCohortMembersEndpoint(
        IAdminGetCohortMembersHandler handler,
        IValidator<AdminGetCohortMembersRequestDto> validator)
    {
        _handler = handler;
        _validator = validator;
    }

    [HttpGet]
    public async Task<IActionResult> Get(Guid cohortId, CancellationToken cancellationToken)
    {
        var dto = new AdminGetCohortMembersRequestDto { CohortId = cohortId };
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);
        var result = await _handler.HandleAsync(dto, cancellationToken);
        return Ok(result);
    }
}