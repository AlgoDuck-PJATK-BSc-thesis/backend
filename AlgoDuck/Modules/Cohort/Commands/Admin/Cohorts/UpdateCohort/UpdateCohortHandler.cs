using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.DTOs;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.UpdateCohort;

public sealed class UpdateCohortHandler : IUpdateCohortHandler
{
    private const int MinNameLength = 3;

    private readonly ApplicationCommandDbContext _db;
    private readonly IValidator<UpdateCohortDto> _validator;

    public UpdateCohortHandler(ApplicationCommandDbContext db, IValidator<UpdateCohortDto> validator)
    {
        _db = db;
        _validator = validator;
    }

    public async Task<CohortItemDto> HandleAsync(Guid cohortId, UpdateCohortDto dto, CancellationToken cancellationToken)
    {
        var validationResult = await _validator.ValidateAsync(dto, cancellationToken);
        if (!validationResult.IsValid)
        {
            var msg = validationResult.Errors.Count > 0 ? validationResult.Errors[0].ErrorMessage : null;
            throw new CohortValidationException(string.IsNullOrWhiteSpace(msg) ? "Invalid cohort update payload." : msg);
        }

        var cohort = await _db.Cohorts.FirstOrDefaultAsync(c => c.CohortId == cohortId, cancellationToken);
        if (cohort is null)
        {
            throw new CohortNotFoundException(cohortId);
        }

        var name = (dto.Name).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new CohortValidationException("Cohort name is required.");
        }

        if (name.Length < MinNameLength)
        {
            throw new CohortValidationException($"Cohort name must be at least {MinNameLength} characters.");
        }

        cohort.Name = name;

        await _db.SaveChangesAsync(cancellationToken);

        return new CohortItemDto
        {
            CohortId = cohort.CohortId,
            Name = cohort.Name,
            IsActive = cohort.IsActive,
            CreatedByUserId = cohort.CreatedByUserId,
            CreatedByDisplay = BuildCreatedByDisplay(cohort.CreatedByUserId, cohort.CreatedByUserLabel),
            CreatedAt = cohort.CreatedAt
        };
    }

    private static string BuildCreatedByDisplay(Guid? createdByUserId, string? createdByUserLabel)
    {
        var label = (createdByUserLabel ?? string.Empty).Trim();

        if (createdByUserId is null)
        {
            if (!string.IsNullOrWhiteSpace(label))
            {
                return $"Deleted user ({label})";
            }

            return "Deleted user";
        }

        if (!string.IsNullOrWhiteSpace(label))
        {
            return label;
        }

        return createdByUserId.Value.ToString();
    }
}
