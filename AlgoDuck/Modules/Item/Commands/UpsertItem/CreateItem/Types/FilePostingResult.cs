using AlgoDuck.Shared.Http;

namespace AlgoDuck.Modules.Item.Commands.CreateItem.Types;
public abstract class FilePostingResult
{
    public required string FileName { get; set; }
    public Status Result { get; protected set; }   
}
