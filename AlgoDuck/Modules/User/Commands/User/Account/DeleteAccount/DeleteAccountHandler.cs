using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.User.Account.DeleteAccount;

public sealed class DeleteAccountHandler : IDeleteAccountHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<DeleteAccountDto> _validator;
    private readonly ApplicationCommandDbContext _commandDbContext;

    public DeleteAccountHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<DeleteAccountDto> validator,
        ApplicationCommandDbContext commandDbContext)
    {
        _userManager = userManager;
        _validator = validator;
        _commandDbContext = commandDbContext;
    }

    public async Task HandleAsync(Guid userId, DeleteAccountDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (userId == Guid.Empty)
        {
            throw new Shared.Exceptions.ValidationException("User identifier is invalid.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException("User not found.");
        }

        var cohortId = user.CohortId;

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.CurrentPassword);
        if (!passwordValid)
        {
            throw new Shared.Exceptions.ValidationException("Invalid password.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Failed to delete account.";
            throw new Shared.Exceptions.ValidationException(errorMessage);
        }

        if (!cohortId.HasValue)
        {
            return;
        }

        var remainingMembers = await _commandDbContext.ApplicationUsers
            .AsNoTracking()
            .CountAsync(u => u.CohortId == cohortId.Value, cancellationToken);

        var cohort = await _commandDbContext.Cohorts
            .FirstOrDefaultAsync(c => c.CohortId == cohortId.Value, cancellationToken);

        if (cohort is null)
        {
            return;
        }

        var now = DateTime.UtcNow;

        if (remainingMembers == 0)
        {
            cohort.IsActive = false;
            if (cohort.EmptiedAt is null)
            {
                cohort.EmptiedAt = now;
            }

            await _commandDbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        if (cohort.EmptiedAt is not null || !cohort.IsActive)
        {
            cohort.EmptiedAt = null;
            cohort.IsActive = true;
            await _commandDbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
