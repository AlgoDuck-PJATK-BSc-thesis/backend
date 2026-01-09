using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.Auth.Shared.DTOs;
using AlgoDuck.Modules.Auth.Shared.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Auth.Commands.Login.Register;

public sealed class RegisterHandler : IRegisterHandler
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IValidator<RegisterDto> _validator;
    private readonly IConfiguration _configuration;
    private readonly ApplicationCommandDbContext _db;

    public RegisterHandler(
        UserManager<ApplicationUser> userManager,
        IEmailSender emailSender,
        IValidator<RegisterDto> validator,
        IConfiguration configuration,
        ApplicationCommandDbContext db)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _validator = validator;
        _configuration = configuration;
        _db = db;
    }

    public async Task<AuthUserDto> HandleAsync(RegisterDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        var existingByUserName = await _userManager.FindByNameAsync(dto.UserName);
        if (existingByUserName is not null)
        {
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException("Username is already taken.");
        }

        var existingByEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingByEmail is not null)
        {
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException("Email is already registered.");
        }

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = false
        };

        var createResult = await _userManager.CreateAsync(user, dto.Password);
        if (!createResult.Succeeded)
        {
            var errors = string.Join("; ", createResult.Errors.Select(e => e.Description));
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException($"User registration failed: {errors}");
        }

        try
        {
            await EnsureAlgoduckOwnedAndSelectedAsync(user.Id, cancellationToken);

            var addRoleResult = await _userManager.AddToRoleAsync(user, "user");
            if (!addRoleResult.Succeeded)
            {
                var errors = string.Join("; ", addRoleResult.Errors.Select(e => e.Description));
                throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException($"Failed to assign default role: {errors}");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = BuildEmailConfirmationLink(user.Id, token);

            await _emailSender.SendEmailConfirmationAsync(user.Id, user.Email, confirmationLink, cancellationToken);

            return new AuthUserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                EmailConfirmed = user.EmailConfirmed
            };
        }
        catch
        {
            await _userManager.DeleteAsync(user);
            throw;
        }
    }

    private async Task EnsureAlgoduckOwnedAndSelectedAsync(Guid userId, CancellationToken cancellationToken)
    {
        var algoduckItemId = await _db.DuckItems
            .Where(i => i.Name.ToLower() == "algoduck")
            .Select(i => i.ItemId)
            .FirstOrDefaultAsync(cancellationToken);

        if (algoduckItemId == Guid.Empty)
        {
            throw new AlgoDuck.Modules.Auth.Shared.Exceptions.ValidationException("Default duck item 'algoduck' was not found.");
        }

        var owned = await _db.DuckOwnerships
            .SingleOrDefaultAsync(o => o.UserId == userId && o.ItemId == algoduckItemId, cancellationToken);

        var selected = await _db.DuckOwnerships
            .Where(o => o.UserId == userId && o.SelectedAsAvatar)
            .ToListAsync(cancellationToken);

        foreach (var s in selected)
        {
            s.SelectedAsAvatar = false;
        }

        if (owned is null)
        {
            owned = new DuckOwnership
            {
                UserId = userId,
                ItemId = algoduckItemId,
                SelectedAsAvatar = true,
                SelectedForPond = true
            };
            _db.DuckOwnerships.Add(owned);
        }
        else
        {
            owned.SelectedAsAvatar = true;
            owned.SelectedForPond = true;
        }

        await _db.SaveChangesAsync(cancellationToken);
    }

    private string BuildEmailConfirmationLink(Guid userId, string token)
    {
        var apiBaseUrl =
            _configuration["App:PublicApiUrl"] ??
            "http://localhost:8080";

        var frontendBaseUrl =
            _configuration["App:FrontendUrl"] ??
            _configuration["CORS:DevOrigins:0"] ??
            "http://localhost:5173";

        apiBaseUrl = apiBaseUrl.TrimEnd('/');
        frontendBaseUrl = frontendBaseUrl.TrimEnd('/');

        var encodedToken = Uri.EscapeDataString(token);
        var encodedUserId = Uri.EscapeDataString(userId.ToString());

        var returnUrl = $"{frontendBaseUrl}/auth/email-confirmed?userId={encodedUserId}&token={encodedToken}";
        var encodedReturnUrl = Uri.EscapeDataString(returnUrl);

        return $"{apiBaseUrl}/auth/email-verification?userId={encodedUserId}&token={encodedToken}&returnUrl={encodedReturnUrl}";
    }
}
