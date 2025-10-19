using Microsoft.AspNetCore.Http.HttpResults;

namespace AlgoDuck.Modules.Cohort.CohortManagement.Queries.GetAllCohorts;

public static class GetAllCohortsEndpoint
{
    public static RouteGroupBuilder MapGetAllCohorts(this RouteGroupBuilder group)
    {
        group.MapGet("/cohorts", async (GetAllCohortsHandler handler, CancellationToken ct) =>
            {
                var data = await handler.HandleAsync(ct);
                return Results.Ok(new { status = "success", data });
            })
            .WithName("CohortManagement.GetAllCohorts");

        return group;
    }
}