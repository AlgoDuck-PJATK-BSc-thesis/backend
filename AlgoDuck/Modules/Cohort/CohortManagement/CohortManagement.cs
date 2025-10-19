using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using AlgoDuck.Modules.Cohort.CohortManagement.Queries.GetAllCohorts;
using AlgoDuck.Modules.Cohort.CohortManagement.Commands.CreateCohort;

namespace AlgoDuck.Modules.Cohort.CohortManagement;

public static class CohortManagement
{
    public static IEndpointRouteBuilder MapCohortManagementEndpoints(this IEndpointRouteBuilder app)
    {
        var adminGroup = app.MapGroup("/api/cohort-management")
            .RequireAuthorization(new AuthorizeAttribute { Roles = "admin" });

        adminGroup
            .MapGetAllCohorts()
            .MapCreateCohort();

        return app;
    }
}