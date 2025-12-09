using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.CreateCohort;

public sealed class CreateCohortHandler : ICreateCohortHandler
{
    private readonly IValidator<CreateCohortDto> _validator;
    private readonly IUserRepository _userRepository;

    public CreateCohortHandler(
        IValidator<CreateCohortDto> validator,
        IUserRepository userRepository)
    {
        _validator = validator;
        _userRepository = userRepository;
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

        var cohort = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = dto.Name,
            IsActive = true,
            CreatedByUserId = userId
        };

        user.CohortId = cohort.CohortId;

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new CreateCohortResultDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            CreatedByUserId = cohort.CreatedByUserId,
            IsActive = cohort.IsActive
        };
    }
}