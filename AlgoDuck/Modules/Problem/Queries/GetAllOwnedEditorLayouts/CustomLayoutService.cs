using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetAllOwnedEditorLayouts;


public interface ICustomLayoutService
{
    public Task<Result<ICollection<LayoutDto>, ErrorObject<string>>> GetCustomLayoutsAsync(Guid userId, CancellationToken cancellationToken =  default);
}

public class CustomLayoutService : ICustomLayoutService
{
    private readonly ICustomLayoutRepository _customLayoutRepository;

    public CustomLayoutService(ICustomLayoutRepository customLayoutRepository)
    {
        _customLayoutRepository = customLayoutRepository;
    }

    public async Task<Result<ICollection<LayoutDto>, ErrorObject<string>>> GetCustomLayoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _customLayoutRepository.GetCustomLayoutsAsync(userId, cancellationToken); 
    }
}