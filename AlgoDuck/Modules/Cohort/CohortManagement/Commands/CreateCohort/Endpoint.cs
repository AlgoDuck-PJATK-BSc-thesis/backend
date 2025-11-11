using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AlgoDuck.Modules.Cohort.CohortManagement.Commands.CreateCohort;

public static class CreateCohortEndpoint
{
    public static RouteGroupBuilder MapCreateCohort(this RouteGroupBuilder group)
    {
        group.MapPost("/cohorts", async (
                CohortCreationDto creationDto,
                ClaimsPrincipal user,
                [FromServices] CreateCohortHandler handler,
                CancellationToken ct) =>
            {
                var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(idStr, out var adminUserId))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(creationDto.Name))
                    return Results.BadRequest(new { status = "error", message = "Name is required.", code = 400 });

                var id = await handler.HandleAsync(creationDto, adminUserId, ct);
                return Results.Created($"/api/cohort-management/cohorts/{id}",
                    new { status = "success", data = new { cohortId = id } });
            })
            .WithName("CohortManagement.CreateCohort");

        return group;
    }
}