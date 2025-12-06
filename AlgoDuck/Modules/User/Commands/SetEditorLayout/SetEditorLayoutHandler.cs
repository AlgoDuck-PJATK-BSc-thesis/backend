using AlgoDuck.DAL;
using AlgoDuck.Models;
using AlgoDuck.Modules.User.Shared.Exceptions;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.User.Commands.SetEditorLayout;

public sealed class SetEditorLayoutHandler : ISetEditorLayoutHandler
{
    private readonly ApplicationCommandDbContext _dbContext;
    private readonly IValidator<SetEditorLayoutDto> _validator;

    public SetEditorLayoutHandler(
        ApplicationCommandDbContext dbContext,
        IValidator<SetEditorLayoutDto> validator)
    {
        _dbContext = dbContext;
        _validator = validator;
    }

    public async Task HandleAsync(Guid userId, SetEditorLayoutDto dto, CancellationToken cancellationToken)
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