using AlgoDuck.DAL;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.UpdatePreferences;

public sealed class UpdatePreferencesHandler : IUpdatePreferencesHandler
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IValidator<UpdatePreferencesDto> _validator;

    public UpdatePreferencesHandler(
        ApplicationCommandDbContext dbContext,
        IValidator<UpdatePreferencesDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, UpdatePreferencesDto dto, CancellationToken cancellationToken)
    {
        await _validator.ValidateAndThrowAsync(dto, cancellationToken);

        if (userId == Guid.Empty)
        {
            throw new Shared.Exceptions.ValidationException("User identifier is invalid.");
        }

        var config = await _dbContext.UserConfigs
            .FirstOrDefaultAsync(c => c.UserId == userId, cancellationToken);

        if (config is null)
        {
            throw new UserNotFoundException("User configuration not found.");
        }

        config.IsDarkMode = dto.IsDarkMode;
        config.IsHighContrast = dto.IsHighContrast;
        config.Language = dto.Language;
        config.EmailNotificationsEnabled = dto.EmailNotificationsEnabled;
        config.PushNotificationsEnabled = dto.PushNotificationsEnabled;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}