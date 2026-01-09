using System.Security.Cryptography;
using AlgoDuck.DAL;
using AlgoDuck.Modules.Cohort.Shared.DTOs;
using AlgoDuck.Modules.Cohort.Shared.Exceptions;
using AlgoDuck.Modules.Cohort.Shared.Interfaces;
using FluentValidation;

namespace AlgoDuck.Modules.Cohort.Commands.Admin.Cohorts.CreateCohort;

public sealed class CreateCohortHandler : ICreateCohortHandler
{
    private static readonly char[] JoinCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789".ToCharArray();

    private readonly ApplicationCommandDbContext _db;
    private readonly ICohortRepository _cohortRepository;
    private readonly IValidator<CreateCohortDto> _validator;

    public CreateCohortHandler(
        ApplicationCommandDbContext db,
        ICohortRepository cohortRepository,
        IValidator<CreateCohortDto> validator)
    {
        _db = db;
        _cohortRepository = cohortRepository;
        _validator = validator;
    }
    
    public async Task<CohortItemDto> HandleAsync(Guid adminUserId, CreateCohortDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var name = (dto.Name).Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new CohortValidationException("Cohort name is required.");
        }

        var joinCode = await GenerateUniqueJoinCodeAsync(10, cancellationToken);
        var now = DateTime.UtcNow;

        var cohort = new Models.Cohort
        {
            CohortId = Guid.NewGuid(),
            Name = name,
            JoinCode = joinCode,
            CreatedByUserId = adminUserId,
            CreatedByUserLabel = null,
            CreatedAt = now,
            IsActive = true,
            EmptiedAt = null
        };

        _db.Cohorts.Add(cohort);
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

    private async Task<string> GenerateUniqueJoinCodeAsync(int length, CancellationToken cancellationToken)
    {
        for (var i = 0; i < 50; i++)
        {
            var candidate = GenerateJoinCode(length);
            var exists = await _cohortRepository.JoinCodeExistsAsync(candidate, cancellationToken);
            if (!exists)
            {
                return candidate;
            }
        }

        throw new CohortValidationException("Failed to generate a unique join code. Please try again.");
    }

    private static string GenerateJoinCode(int length)
    {
        var bytes = RandomNumberGenerator.GetBytes(length);
        var chars = new char[length];

        for (var i = 0; i < length; i++)
        {
            chars[i] = JoinCodeAlphabet[bytes[i] % JoinCodeAlphabet.Length];
        }

        return new string(chars);
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
