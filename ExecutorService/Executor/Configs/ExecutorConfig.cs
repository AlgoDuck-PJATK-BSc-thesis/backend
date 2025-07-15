using ExecutorService.Executor._ExecutorUtils;

namespace ExecutorService.Executor.Configs;

public interface IExecutorConfig
{
    public Language[] GetSupportedLanguages();
}


// TODO add support for win/mac based bare metal execution (purely for the dev environment)
public class ExecutorConfig : IExecutorConfig
{
    private readonly Language[] _supportedLanguages;

    public Language[] GetSupportedLanguages()
    {
        return _supportedLanguages;
    }

    public ExecutorConfig()
    {
        IExecutorRepository executorRepository = new ExecutorRepositoryMock();
        _supportedLanguages = executorRepository.GetSupportedLangsAsync().Result; 
    }
}
