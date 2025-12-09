using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Commands.CohortManagement.UpdateCohort;

public sealed class UpdateCohortHandler : IUpdateCohortHandler
{
    private readonly IValidator<UpdateCohortDto> _validator;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationCommandDbContext _commandDbContext;

    public UpdateCohortHandler(
        IValidator<UpdateCohortDto> validator,
        IUserRepository userRepository,
        ApplicationCommandDbContext commandDbContext)
    {
        _validator = validator;
        _userRepository = userRepository;
        _commandDbContext = commandDbContext;
    }

    public async Task<UpdateCohortResultDto> HandleAsync(
        Guid userId,
        Guid cohortId,
        UpdateCohortDto dto,
        CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new CohortValidationException("Invalid cohort update payload.");
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            throw new CohortValidationException("User not found.");
        }

        if (!user.CohortId.HasValue || user.CohortId.Value != cohortId)
        {
            throw new CohortValidationException("User does not belong to this cohort.");
        }

        var cohort = await _commandDbContext.Cohorts
            .FirstOrDefaultAsync(c => c.CohortId == cohortId, cancellationToken);

        if (cohort is null)
        {
            throw new CohortNotFoundException(cohortId);
        }

        if (!cohort.IsActive)
        {
            throw new CohortValidationException("Cohort is not active.");
        }

        cohort.Name = dto.Name;

        await _commandDbContext.SaveChangesAsync(cancellationToken);

        return new UpdateCohortResultDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            CreatedByUserId = cohort.CreatedByUserId,
            IsActive = cohort.IsActive
        };
    }
}