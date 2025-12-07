namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

public class FilePosition
{
    private int _filePos;

    private Stack<int> Checkpoints { get; init; } = [];

    private FilePosition(int filePos)
    {
        _filePos = filePos;
    }

    public static FilePosition GetFilePosition(out FilePosition fp, int filePos = 0)
    {
        fp = new FilePosition(filePos);
        return fp;
    }

    public void CreateCheckpoint()
    {
        Checkpoints.Push(_filePos);
    }

    public void LoadCheckpoint()
    {
        _filePos = Checkpoints.Pop();
    }

    public int GetFilePos()
    {
        return _filePos;
    }

    public void IncrementFilePos(int times = 1)
    {
        _filePos += times;
    }
}