using System.Text;

namespace ExecutorService.Executor.Types;

public class UserSolutionData(Guid executionId, string signingKey, string lang, string funcName, StringBuilder fileContents, string? exerciseId)
{
    public Guid ExecutionId => executionId;

    public string SigningKey => signingKey; // TODO maybe could just use ExecutionId?
    public string Lang => lang;

    public string FuncName => funcName;

    public StringBuilder FileContents => fileContents;

    public string? ExerciseId => exerciseId;
}