using AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohort;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.User.Management.JoinCohortByCode;

public sealed class JoinCohortByCodeHandler : IJoinCohortByCodeHandler
{
    private readonly IValidator<JoinCohortByCodeDto> _validator;
    private readonly ICohortRepository _cohortRepository;
    private readonly IUserRepository _userRepository;

    public JoinCohortByCodeHandler(
        IValidator<JoinCohortByCodeDto> validator,
        ICohortRepository cohortRepository,
        IUserRepository userRepository)
    {
        _validator = validator;
        _cohortRepository = cohortRepository;
        _userRepository = userRepository;
    }

    public async Task<JoinCohortResultDto> HandleAsync(Guid userId, JoinCohortByCodeDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid join code.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new CohortValidationException("User not found.");
        }

        if (user.CohortId.HasValue)
        {
            throw new CohortValidationException("Leave current cohort to join another.");
        }

        var code = dto.Code.Trim().ToUpperInvariant();
        var cohort = await _cohortRepository.GetByJoinCodeAsync(code, cancellationToken);
        if (cohort is null || !cohort.IsActive)
        {
            throw new CohortValidationException("Cohort not found.");
        }

        user.CohortId = cohort.CohortId;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new JoinCohortResultDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            IsActive = cohort.IsActive
        };
    }
}