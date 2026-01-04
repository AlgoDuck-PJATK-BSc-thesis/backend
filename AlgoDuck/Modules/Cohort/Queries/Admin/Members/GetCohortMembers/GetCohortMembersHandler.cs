using AlgoDuck.DAL;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Queries.Admin.Members.GetCohortMembers;

public sealed class GetCohortMembersHandler : IAdminGetCohortMembersHandler
{
    private readonly ApplicationQueryDbContext _queryDbContext;

    public GetCohortMembersHandler(ApplicationQueryDbContext queryDbContext)
    {
        _queryDbContext = queryDbContext;
    }

    public async Task<GetCohortMembersResultDto> HandleAsync(
        GetCohortMembersRequestDto dto,
        CancellationToken cancellationToken)
    {
        var members = await _queryDbContext.ApplicationUsers
            .Where(u => u.CohortId == dto.CohortId)
            .OrderBy(u => u.UserName)
            .Select(u => new AdminGetCohortMembersItemDto
            {
                UserId = u.Id,
                UserName = (u.UserName ?? string.Empty).Trim(),
                Email = (u.Email ?? string.Empty).Trim(),
                JoinedAt = u.CohortJoinedAt
            })
            .ToListAsync(cancellationToken);

        return new GetCohortMembersResultDto
        {
            CohortId = dto.CohortId,
            TotalMembers = members.Count,
            Members = members
        };
    }
}