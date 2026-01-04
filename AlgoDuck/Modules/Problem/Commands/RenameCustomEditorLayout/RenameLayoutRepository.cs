using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.RenameCustomEditorLayout;

public interface IRenameLayoutRepository
{
    public Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto renameRequest,
        CancellationToken cancellationToken = default);
}

public class RenameLayoutRepository(
    ApplicationCommandDbContext dbContext
) : IRenameLayoutRepository
{
    public async Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto renameRequest,
        CancellationToken cancellationToken = default)
    {
        var rowsChanged = await dbContext.EditorLayouts
            .Include(el => el.UserConfig)
            .Where(e => e.EditorLayoutId == renameRequest.LayoutId && e.UserConfig.UserId == renameRequest.UserId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(e => e.LayoutName, renameRequest.NewName),
                cancellationToken: cancellationToken);
        if (rowsChanged == 0)
            return Result<RenameLayoutResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Could not attribute layout: {renameRequest.LayoutId} to user: {renameRequest.UserId}"));
        
        return Result<RenameLayoutResultDto, ErrorObject<string>>.Ok(new RenameLayoutResultDto
        {
            LayoutId = renameRequest.LayoutId,
            NewName = renameRequest.NewName,
        });

    }
}
