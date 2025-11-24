namespace AlgoDuck.Shared.Analyzer._AnalyzerUtils.Types;

public class FilePosition(int filePos = 0)
{
    private int _filePos = filePos;

    public int GetFilePos()
    {
        return _filePos;
    }

    public void IncrementFilePos(int times = 1)
    {
        _filePos += times;
    }

}