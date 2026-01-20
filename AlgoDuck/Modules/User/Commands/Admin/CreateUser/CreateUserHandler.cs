using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Shared.Utilities;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.Admin.CreateUser;

public sealed class CreateUserHandler : ICreateUserHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IValidator<CreateUserDto> _validator;
    private readonly IDefaultDuckService _defaultDuckService;
    private readonly ApplicationCommandDbContext _dbContext;

    public CreateUserHandler(
        UserManager<ApplicationUser> userManager,
        IValidator<CreateUserDto> validator,
        IDefaultDuckService defaultDuckService,
        ApplicationCommandDbContext dbContext)
    {
        _userManager = userManager;
        _validator = validator;
        _defaultDuckService = defaultDuckService;
        _dbContext = dbContext;
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
        }.EnrichWithDefaults();

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

        if (roleToAssign != "admin")
        {
            await _defaultDuckService.EnsureAlgoduckOwnedAndSelectedAsync(user.Id, cancellationToken);
        }

        await EnsureUserConfigExistsAsync(user.Id, cancellationToken);

        return new CreateUserResultDto
        {
            UserId = user.Id,
            Email = user.Email ?? string.Empty,
            Username = user.UserName ?? string.Empty,
            Role = roleToAssign,
            EmailVerified = user.EmailConfirmed
        };
    }

    private async Task EnsureUserConfigExistsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.UserConfigs
            .AsNoTracking()
            .AnyAsync(c => c.UserId == userId, cancellationToken);

        if (exists)
        {
            return;
        }

        _dbContext.UserConfigs.Add(new UserConfig
        {
            UserId = userId,
            EditorFontSize = 11,
            EmailNotificationsEnabled = false,
            IsDarkMode = true,
            IsHighContrast = false
        });

        await _dbContext.SaveChangesAsync(cancellationToken);
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
