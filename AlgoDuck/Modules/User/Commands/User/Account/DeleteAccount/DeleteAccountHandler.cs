using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Exceptions;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.User.Commands.User.Account.DeleteAccount;

public sealed class DeleteAccountHandler : IDeleteAccountHandler
{
    private readonly UserManager<ApplicationUser> _userManager;

    public DeleteAccountHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task HandleAsync(Guid userId, DeleteAccountDto dto, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (userId == Guid.Empty)
        {
            throw new ValidationException("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException("User not found.");
        }

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new ValidationException($"Could not delete account: {errors}");
        }
    }
}