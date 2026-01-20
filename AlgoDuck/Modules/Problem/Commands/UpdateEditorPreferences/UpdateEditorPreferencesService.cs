using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Problem.Commands.UpdateEditorPreferences;

public interface IUpdateEditorPreferencesService
{
    public Task<Result<PreferencesUpdateResultDto, ErrorObject<string>>> UpdateEditorPreferencesAsync(
        PreferencesUpdateRequestDto requestDto, CancellationToken cancellationToken = default);
}

public class UpdateEditorPreferencesService : IUpdateEditorPreferencesService
{
    private readonly IUpdateEditorPreferencesRepository _repository;

    public UpdateEditorPreferencesService(IUpdateEditorPreferencesRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PreferencesUpdateResultDto, ErrorObject<string>>> UpdateEditorPreferencesAsync(
        PreferencesUpdateRequestDto requestDto,
        CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateEditorPreferencesAsync(requestDto, cancellationToken);
    }
}
