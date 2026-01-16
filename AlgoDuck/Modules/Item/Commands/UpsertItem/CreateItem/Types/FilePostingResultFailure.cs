using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.CreateItem.Types;


public class FilePostingFailureResult : FilePostingResult
{
    public required string Reason { get; set; }
    public FilePostingFailureResult()
    {
        Result = Status.Error;
    }
}
