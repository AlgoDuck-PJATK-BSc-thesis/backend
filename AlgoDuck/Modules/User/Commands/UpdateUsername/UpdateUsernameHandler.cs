using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.User.Commands.UpdateUsername;

public sealed class UpdateUsernameHandler : IUpdateUsernameHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UpdateUsernameDto> _validator;

    public UpdateUsernameHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<UpdateUsernameDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, UpdateUsernameDto dto, CancellationToken cancellationToken)
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

        var existing = await _userManager.FindByNameAsync(dto.NewUserName);
        if (existing is not null && existing.Id != user.Id)
        {
            throw new Shared.Exceptions.ValidationException("Username is already taken.");
        }

        user.UserName = dto.NewUserName;

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errorMessage = result.Errors.FirstOrDefault()?.Description ?? "Failed to update username.";
            throw new Shared.Exceptions.ValidationException(errorMessage);
        }
    }
}