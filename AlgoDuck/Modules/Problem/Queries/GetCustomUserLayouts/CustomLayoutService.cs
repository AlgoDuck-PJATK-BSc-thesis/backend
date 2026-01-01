using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetCustomUserLayouts;


public interface ICustomLayoutService
{
    public Task<Result<ICollection<LayoutDto>, ErrorObject<string>>> GetCustomLayoutsAsync(Guid userId, CancellationToken cancellationToken =  default);
}

public class CustomLayoutService(
    ICustomLayoutRepository customLayoutRepository
    ) : ICustomLayoutService
{
    public async Task<Result<ICollection<LayoutDto>, ErrorObject<string>>> GetCustomLayoutsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await customLayoutRepository.GetCustomLayoutsAsync(userId, cancellationToken); 
    }
}