using AlgoDuck.Models;
using FluentValidation;
using Microsoft.AspNetCore.Identity;

namespace AlgoDuck.Modules.User.Commands.CreateUser;

public sealed class CreateUserHandler : ICreateUserHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<CreateUserDto> _validator;

    public CreateUserHandler(UserManager<ApplicationUser> userManager, IValidator<CreateUserDto> validator)
    {
        _userManager = userManager;
        _validator = validator;
    }

    public async Task<CreateUserResultDto> HandleAsync(CreateUserDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var role = dto.Role.Trim().ToLowerInvariant();
        var email = dto.Email.Trim();

        var existingByEmail = await _userManager.FindByEmailAsync(email);
        if (existingByEmail is not null)
        {
            throw new ValidationException("Email already exists.");
        }

        var requestedUsername = dto.Username is null ? null : dto.Username.Trim();
        var username = string.IsNullOrWhiteSpace(requestedUsername)
            ? await GenerateUniqueUsernameAsync(role, cancellationToken)
            : await EnsureUniqueUsernameAsync(requestedUsername, cancellationToken);

        var user = new ApplicationUser
        {
            UserName = username,
            Email = email,
            EmailConfirmed = dto.EmailVerified
        };

        var create = await _userManager.CreateAsync(user, dto.Password);
        if (!create.Succeeded)
        {
            var msg = create.Errors.FirstOrDefault()?.Description ?? "Failed to create user.";
            throw new ValidationException(msg);
        }

        var roleToAssign = role == "admin" ? "admin" : "user";
        var addRole = await _userManager.AddToRoleAsync(user, roleToAssign);
        if (!addRole.Succeeded)
        {
            var msg = addRole.Errors.FirstOrDefault()?.Description ?? "Failed to assign role.";
            throw new ValidationException(msg);
        }

        return new CreateUserResultDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            Role = roleToAssign,
            EmailVerified = user.EmailConfirmed
        };
    }

    private async Task<string> EnsureUniqueUsernameAsync(string username, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var existing = await _userManager.FindByNameAsync(username);
        if (existing is null) return username;
        throw new ValidationException("Username already exists.");
    }

    private async Task<string> GenerateUniqueUsernameAsync(string role, CancellationToken cancellationToken)
    {
        for (var i = 0; i < 80; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var candidate = role == "admin"
                ? UsernameGenerator.GenerateAdminStyle()
                : UsernameGenerator.GenerateUserStyle();

            var existing = await _userManager.FindByNameAsync(candidate);
            if (existing is null) return candidate;
        }

        throw new ValidationException("Failed to generate a unique username.");
    }
}
