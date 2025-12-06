using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.User.Commands.DeleteAccount;

public sealed class DeleteAccountHandler : IDeleteAccountHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<DeleteAccountDto> _validator;

    public DeleteAccountHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<DeleteAccountDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
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
    }
}