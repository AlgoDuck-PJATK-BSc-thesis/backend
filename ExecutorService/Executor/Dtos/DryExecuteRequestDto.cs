using ExecutorService.Executor.Types;

namespace ExecutorService.Executor.Dtos;

public class DryExecuteRequestDto : IExecutionRequestBase
{
    public string CodeB64 { get; set; } = string.Empty;
    public string Lang { get; set; } = string.Empty;
    public string GetCodeB64()
    {
        return CodeB64;
    }

    public string GetLang()
    {
        return Lang;
    }
}