using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.CreateEditorLayout;


public interface ICreateLayoutService
{
    public Task<Result<LayoutCreateResultDto, ErrorObject<string>>> CreateLayoutAsync(LayoutCreateDto createDto,
        CancellationToken cancellationToken = default);
}

public class CreateLayoutService: ICreateLayoutService
{
    private readonly ICreateLayoutRepository _createLayoutRepository;

    public CreateLayoutService(ICreateLayoutRepository createLayoutRepository)
    {
        _createLayoutRepository = createLayoutRepository;
    }

    public async Task<Result<LayoutCreateResultDto, ErrorObject<string>>> CreateLayoutAsync(LayoutCreateDto createDto, CancellationToken cancellationToken = default)
    {
        return await _createLayoutRepository.GetOwnedLayoutCountAsync(createDto.UserId, cancellationToken).BindAsync(async
            editorLayout =>
        {
            if (editorLayout.Count == 13)
                return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Cannot own more than 10 layouts. Consider deleting one"));
            
            if (editorLayout.Select(l => l.Name).Contains(createDto.LayoutName))
            {
                return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(
                    ErrorObject<string>.BadRequest("Duplicate layout name"));
            }
            return await _createLayoutRepository.CreateLayoutAsync(createDto, cancellationToken);
        });
    }
}
