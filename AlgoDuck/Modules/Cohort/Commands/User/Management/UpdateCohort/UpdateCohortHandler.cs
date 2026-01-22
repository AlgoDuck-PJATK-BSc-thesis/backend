using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Commands.User.Management.UpdateCohort;

public sealed class UpdateCohortHandler : IUpdateCohortHandler
{
    private const int MinNameLength = 3;

    private readonly IValidator<UpdateCohortDto> _validator;
    private readonly IUserRepository _userRepository;
    private readonly ApplicationCommandDbContext _commandDbContext;
    private readonly IChatModerationService _chatModerationService;

    public UpdateCohortHandler(
        IValidator<UpdateCohortDto> validator,
        IUserRepository userRepository,
        ApplicationCommandDbContext commandDbContext,
        IChatModerationService chatModerationService)
    {
        _validator = validator;
        _userRepository = userRepository;
        _commandDbContext = commandDbContext;
        _chatModerationService = chatModerationService;
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
            var msg = validationResult.Errors.Count > 0 ? validationResult.Errors[0].ErrorMessage : null;
            throw new CohortValidationException(string.IsNullOrWhiteSpace(msg) ? "Invalid cohort update payload." : msg);
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

        var nameToSet = (dto.Name).Trim();
        if (string.IsNullOrWhiteSpace(nameToSet))
        {
            throw new CohortValidationException("Cohort name is required.");
        }

        if (nameToSet.Length < MinNameLength)
        {
            throw new CohortValidationException($"Cohort name must be at least {MinNameLength} characters.");
        }

        var moderationResult = await _chatModerationService.CheckMessageAsync(
            userId,
            cohortId,
            nameToSet,
            cancellationToken);

        if (!moderationResult.IsAllowed)
        {
            throw new ChatValidationException(
                moderationResult.BlockReason ?? "This name violates our content rules.",
                moderationResult.Category);
        }

        cohort.Name = nameToSet;

        await _commandDbContext.SaveChangesAsync(cancellationToken);

        return new UpdateCohortResultDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            CreatedByUserId = cohort.CreatedByUserId.GetValueOrDefault(),
            IsActive = cohort.IsActive
        };
    }
}
