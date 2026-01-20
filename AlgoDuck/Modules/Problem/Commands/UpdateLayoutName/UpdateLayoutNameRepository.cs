using AlgoDuck.DAL;
using AlgoDuck.Shared.Http;
using Microsoft.EntityFrameworkCore;

namespace AlgoDuck.Modules.Problem.Commands.UpdateLayoutName;

public interface IUpdateLayoutNameRepository
{
    public Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto requestDto,
        CancellationToken cancellationToken = default);
}

public class UpdateLayoutNameRepository : IUpdateLayoutNameRepository
{
    private readonly ApplicationCommandDbContext _dbContext;

    public UpdateLayoutNameRepository(ApplicationCommandDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<RenameLayoutResultDto, ErrorObject<string>>> RenameLayoutAsync(RenameLayoutRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        var rowsChanged = await _dbContext.EditorLayouts.Include(l => l.OwnedBy)
            .Where(l => l.OwnedBy.Select(ol => ol.UserId).Contains(requestDto.UserId) &&
                        l.EditorLayoutId == requestDto.LayoutId && !l.IsDefaultLayout).ExecuteUpdateAsync(
                setters => setters.SetProperty(e => e.LayoutName, requestDto.NewName),
                cancellationToken: cancellationToken);
        if (rowsChanged == 0)
            return Result<RenameLayoutResultDto, ErrorObject<string>>.Err(ErrorObject<string>.NotFound($"Could not attribute layout: {requestDto.LayoutId} to user: {requestDto.UserId}"));
        return Result<RenameLayoutResultDto, ErrorObject<string>>.Ok(new RenameLayoutResultDto
        {
            LayoutId = requestDto.LayoutId,
            NewName = requestDto.NewName
        });
    }
}