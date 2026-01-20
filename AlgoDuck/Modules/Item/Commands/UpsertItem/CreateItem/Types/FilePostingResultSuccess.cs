using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.CreateItem.Types;

public class FilePostingSuccessResult : FilePostingResult
{
    public FilePostingSuccessResult()
    {
        Result = Status.Success;
    }
}
