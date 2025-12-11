using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortMembers;

public sealed class GetCohortMembersHandler : IGetCohortMembersHandler
{
    private readonly IValidator<GetCohortMembersRequestDto> _validator;
    private readonly ICohortRepository _cohortRepository;
    private readonly IProfileService _profileService;
    private readonly ApplicationQueryDbContext _queryDbContext;

    public GetCohortMembersHandler(
        IValidator<GetCohortMembersRequestDto> validator,
        ICohortRepository cohortRepository,
        IProfileService profileService,
        ApplicationQueryDbContext queryDbContext)
    {
        _validator = validator;
        _cohortRepository = cohortRepository;
        _profileService = profileService;
        _queryDbContext = queryDbContext;
    }

    public async Task<GetCohortMembersResultDto> HandleAsync(
        Guid userId,
        GetCohortMembersRequestDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid cohort members query.");
        }

        var belongs = await _cohortRepository.UserBelongsToCohortAsync(userId, dto.CohortId, cancellationToken);
        if (!belongs)
        {
            throw new CohortValidationException("User does not belong to this cohort.");
        }

        var baseQuery = _queryDbContext.ApplicationUsers
            .Where(u => u.CohortId == dto.CohortId)
            .OrderBy(u => u.UserName);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var skip = (dto.Page - 1) * dto.PageSize;

        var users = await baseQuery
            .Skip(skip)
            .Take(dto.PageSize)
            .ToListAsync(cancellationToken);

        var items = new List<GetCohortMembersItemDto>(users.Count);

        foreach (var user in users)
        {
            var profile = await _profileService.GetProfileAsync(user.Id, cancellationToken);

            items.Add(new GetCohortMembersItemDto
            {
                UserId = user.Id,
                UserName = profile.Username,
                UserAvatarUrl = profile.S3AvatarUrl,
                IsYou = user.Id == userId
            });
        }

        return new GetCohortMembersResultDto
        {
            Items = items,
            Page = dto.Page,
            PageSize = dto.PageSize,
            TotalCount = totalCount
        };
    }
}