using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.User.Commands.Admin.UpdateUser;

public sealed class UpdateUserHandler : IUpdateUserHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<UpdateUserDto> _validator;

    public UpdateUserHandler(UserManager<ApplicationUser> userManager, IValidator<UpdateUserDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<UpdateUserResultDto> HandleAsync(Guid userId, UpdateUserDto dto, CancellationToken cancellationToken)
    {
        if (userId == Guid.Empty)
        {
            throw new Shared.Exceptions.ValidationException("User identifier is invalid.");
        }

        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            throw new UserNotFoundException("User not found.");
        }

        if (dto.Username is not null)
        {
            var newUsername = dto.Username.Trim();

            var existing = await _userManager.FindByNameAsync(newUsername);
            if (existing is not null && existing.Id != user.Id)
            {
                throw new Shared.Exceptions.ValidationException("Username already exists.");
            }

            var setName = await _userManager.SetUserNameAsync(user, newUsername);
            if (!setName.Succeeded)
            {
                var msg = setName.Errors.FirstOrDefault()?.Description ?? "Failed to update username.";
                throw new Shared.Exceptions.ValidationException(msg);
            }
        }

        if (dto.Email is not null)
        {
            var newEmail = dto.Email.Trim();

            var existingEmail = await _userManager.FindByEmailAsync(newEmail);
            if (existingEmail is not null && existingEmail.Id != user.Id)
            {
                throw new Shared.Exceptions.ValidationException("Email already exists.");
            }

            var setEmail = await _userManager.SetEmailAsync(user, newEmail);
            if (!setEmail.Succeeded)
            {
                var msg = setEmail.Errors.FirstOrDefault()?.Description ?? "Failed to update email.";
                throw new Shared.Exceptions.ValidationException(msg);
            }
        }

        if (dto.Password is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var reset = await _userManager.ResetPasswordAsync(user, token, dto.Password);
            if (!reset.Succeeded)
            {
                var msg = reset.Errors.FirstOrDefault()?.Description ?? "Failed to update password.";
                throw new Shared.Exceptions.ValidationException(msg);
            }
        }

        if (dto.Role is not null)
        {
            var target = dto.Role.Trim().ToLowerInvariant();
            var current = await _userManager.GetRolesAsync(user);

            var normalized = current.Select(r => r.ToLowerInvariant()).ToList();

            var hasTarget = normalized.Contains(target);
            var toRemove = current.Where(r =>
            {
                var v = r.ToLowerInvariant();
                return (v == "user" || v == "admin") && v != target;
            }).ToList();

            if (toRemove.Count > 0)
            {
                var rm = await _userManager.RemoveFromRolesAsync(user, toRemove);
                if (!rm.Succeeded)
                {
                    var msg = rm.Errors.FirstOrDefault()?.Description ?? "Failed to update roles.";
                    throw new Shared.Exceptions.ValidationException(msg);
                }
            }

            if (!hasTarget)
            {
                var add = await _userManager.AddToRoleAsync(user, target);
                if (!add.Succeeded)
                {
                    var msg = add.Errors.FirstOrDefault()?.Description ?? "Failed to update roles.";
                    throw new Shared.Exceptions.ValidationException(msg);
                }
            }
        }

        var roles = (await _userManager.GetRolesAsync(user)).ToList();

        return new UpdateUserResultDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            Roles = roles
        };
    }
}
