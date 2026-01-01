using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.CreateEditorLayout;


public interface ICreateLayoutService
{
    public Task<Result<LayoutCreateResultDto, ErrorObject<string>>> CreateLayoutAsync(LayoutCreateDto createDto,
        CancellationToken cancellationToken = default);
}

public class CreateLayoutService(
    ICreateLayoutRepository createLayoutRepository
    ) : ICreateLayoutService
{
    public async Task<Result<LayoutCreateResultDto, ErrorObject<string>>> CreateLayoutAsync(LayoutCreateDto createDto, CancellationToken cancellationToken = default)
    {

        var userLayoutsResult = await createLayoutRepository.GetOwnedLayoutCountAsync(createDto.UserId, cancellationToken);
        if (userLayoutsResult.IsErr)
            return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(userLayoutsResult.AsT1);
        
        var userLayouts = userLayoutsResult.AsT0;

        if (userLayouts.Count == 10)
            return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Cannot own more than 10 layouts"));
        
        if (userLayouts.Select(l => l.Name).Contains(createDto.LayoutName))
            return Result<LayoutCreateResultDto, ErrorObject<string>>.Err(ErrorObject<string>.BadRequest("Duplicate layout name"));
        
        return await createLayoutRepository.CreateLayoutAsync(createDto, cancellationToken);
    }
}
