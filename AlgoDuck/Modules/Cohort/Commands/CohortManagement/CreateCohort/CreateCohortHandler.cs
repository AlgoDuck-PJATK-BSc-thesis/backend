using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;

public sealed class CreateCohortHandler : ICreateCohortHandler
{
    private readonly IValidator<CreateCohortDto> _validator;
    private readonly IUserRepository _userRepository;
    private readonly ICohortRepository _cohortRepository;

    public CreateCohortHandler(
        IValidator<CreateCohortDto> validator,
        IUserRepository userRepository,
        ICohortRepository cohortRepository)
    {
        _validator = validator;
        _userRepository = userRepository;
        _cohortRepository = cohortRepository;
    }

    public async Task<CreateCohortResultDto> HandleAsync(Guid userId, CreateCohortDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid cohort data.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new CohortValidationException("User not found.");
        }

        if (user.CohortId.HasValue)
        {
            throw new CohortValidationException("User already belongs to a cohort.");
        }

        var joinCode = await CohortJoinCodeGenerator.GenerateUniqueAsync(_cohortRepository, 8, cancellationToken);

        var cohort = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = dto.Name,
            IsActive = true,
            CreatedByUserId = userId,
            JoinCode = joinCode
        };

        await _cohortRepository.AddAsync(cohort, cancellationToken);

        user.CohortId = cohort.CohortId;
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new CreateCohortResultDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            CreatedByUserId = cohort.CreatedByUserId.GetValueOrDefault(),
            IsActive = cohort.IsActive,
            JoinCode = cohort.JoinCode
        };
    }
}