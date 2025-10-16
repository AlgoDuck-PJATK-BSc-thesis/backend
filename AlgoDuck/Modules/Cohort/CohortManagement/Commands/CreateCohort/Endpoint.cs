using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AlgoDuck.Modules.Cohort.CohortManagement.Commands.CreateCohort;

public static class CreateCohortEndpoint
{
    public static RouteGroupBuilder MapCreateCohort(this RouteGroupBuilder group)
    {
        group.MapPost("/cohorts", async (
                CreateCohortDto dto,
                ClaimsPrincipal user,
                CreateCohortHandler handler,
                CancellationToken ct) =>
            {
                var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(idStr, out var adminUserId))
                    return Results.Unauthorized();

                if (string.IsNullOrWhiteSpace(dto.Name))
                    return Results.BadRequest(new { status = "error", message = "Name is required.", code = 400 });

                var id = await handler.HandleAsync(dto, adminUserId, ct);
                return Results.Created($"/api/cohort-management/cohorts/{id}",
                    new { status = "success", data = new { cohortId = id } });
            })
            .WithName("CohortManagement.CreateCohort");

        return group;
    }
}