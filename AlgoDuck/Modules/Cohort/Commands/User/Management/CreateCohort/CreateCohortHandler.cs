using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using AlgoDuck.Modules.Cohort.Shared.Utils;
using AlgoDuck.Modules.User.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.User.Management.CreateCohort;

public sealed class CreateCohortHandler : ICreateCohortHandler
{
    private readonly IValidator<CreateCohortDto> _validator;
    private readonly IUserRepository _userRepository;
    private readonly ICohortRepository _cohortRepository;
    private readonly IChatModerationService _chatModerationService;

    public CreateCohortHandler(
        IValidator<CreateCohortDto> validator,
        IUserRepository userRepository,
        ICohortRepository cohortRepository,
        IChatModerationService chatModerationService)
    {
        _validator = validator;
        _userRepository = userRepository;
        _cohortRepository = cohortRepository;
        _chatModerationService = chatModerationService;
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
            throw new CohortValidationException("Leave current cohort to join another.");
        }

        var nameToSet = (dto.Name).Trim();
        if (string.IsNullOrWhiteSpace(nameToSet))
        {
            throw new CohortValidationException("Invalid cohort data.");
        }

        var cohortId = Guid.NewGuid();

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

        var now = DateTime.UtcNow;

        var createdByLabel =
            !string.IsNullOrWhiteSpace(user.UserName) ? user.UserName.Trim() :
            !string.IsNullOrWhiteSpace(user.Email) ? user.Email.Trim() :
            userId.ToString();

        var joinCode = await CohortJoinCodeGenerator.GenerateUniqueAsync(_cohortRepository, 8, cancellationToken);

        var cohort = new Models.Cohort
        {
            CohortId = cohortId,
            Name = nameToSet,
            IsActive = true,
            CreatedByUserId = userId,
            CreatedByUserLabel = createdByLabel,
            EmptiedAt = null,
            JoinCode = joinCode
        };

        await _cohortRepository.AddAsync(cohort, cancellationToken);

        user.CohortId = cohort.CohortId;
        user.CohortJoinedAt = now;
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
