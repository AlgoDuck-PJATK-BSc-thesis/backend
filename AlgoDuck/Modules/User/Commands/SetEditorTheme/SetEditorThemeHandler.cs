using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.SetEditorTheme;

public sealed class SetEditorThemeHandler : ISetEditorThemeHandler
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IValidator<SetEditorThemeDto> _validator;

    public SetEditorThemeHandler(
        ApplicationCommandDbContext dbContext,
        IValidator<SetEditorThemeDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, SetEditorThemeDto dto, CancellationToken cancellationToken)
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

        var theme = await _dbContext.EditorThemes
            .FirstOrDefaultAsync(t => t.EditorThemeId == dto.EditorThemeId, cancellationToken);

        if (theme is null)
        {
            throw new Shared.Exceptions.ValidationException("Editor theme not found.");
        }

        var layout = await _dbContext.EditorLayouts
            .FirstOrDefaultAsync(l => l.UserConfigId == config.UserId, cancellationToken);

        if (layout is null)
        {
            layout = new EditorLayout
            {
                EditorLayoutId = Guid.NewGuid(),
                UserConfigId = config.UserId,
                EditorThemeId = dto.EditorThemeId
            };

            await _dbContext.EditorLayouts.AddAsync(layout, cancellationToken);
        }
        else
        {
            layout.EditorThemeId = dto.EditorThemeId;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}