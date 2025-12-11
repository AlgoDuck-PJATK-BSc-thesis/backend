using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Queries.GetCohortById;

public sealed class GetCohortByIdHandler : IGetCohortByIdHandler
{
    private readonly IValidator<GetCohortByIdRequestDto> _validator;
    private readonly ICohortRepository _cohortRepository;
    private readonly IUserRepository _userRepository;

    public GetCohortByIdHandler(
        IValidator<GetCohortByIdRequestDto> validator,
        ICohortRepository cohortRepository,
        IUserRepository userRepository)
    {
        _validator = validator;
        _cohortRepository = cohortRepository;
        _userRepository = userRepository;
    }

    public async Task<GetCohortByIdResultDto> HandleAsync(
        Guid userId,
        GetCohortByIdRequestDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid cohort id.");
        }

        var cohort = await _cohortRepository.GetByIdAsync(dto.CohortId, cancellationToken);
        if (cohort is null || !cohort.IsActive)
        {
            throw new CohortNotFoundException(dto.CohortId);
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        var isMember = user?.CohortId == cohort.CohortId;

        return new GetCohortByIdResultDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            IsActive = cohort.IsActive,
            CreatedByUserId = cohort.CreatedByUserId,
            IsMember = isMember
        };
    }
}