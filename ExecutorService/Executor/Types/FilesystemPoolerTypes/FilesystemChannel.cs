using System.Threading.Channels;

namespace ExecutorService.Executor.Types.FilesystemPoolerTypes;

internal sealed class FilesystemChannel<T>(Channel<T> channel)
{
    internal ChannelReader<T> Reader { get; } = channel.Reader;
    internal ChannelWriter<T> Writer { get; } = channel.Writer;
}