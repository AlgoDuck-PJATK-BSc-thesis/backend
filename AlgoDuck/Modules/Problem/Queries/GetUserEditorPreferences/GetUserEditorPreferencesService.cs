using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Queries.GetUserEditorPreferences;

public interface IGetUserEditorPreferencesService
{
    public Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId,
        CancellationToken cancellationToken = default);
}

public class GetUserEditorPreferencesService : IGetUserEditorPreferencesService
{
    private readonly IGetUserEditorPreferencesRepository _repository;

    public GetUserEditorPreferencesService(IGetUserEditorPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<UserEditorPreferencesDto, ErrorObject<string>>> GetUserEditorPreferencesAsync(Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetUserEditorPreferencesAsync(userId, cancellationToken);
    }
}