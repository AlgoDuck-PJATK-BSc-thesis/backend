using System.Text;

namespace ExecutorService.Executor._ExecutorUtils;

public class UserSolutionData(Guid executionId, string signingKey, Language lang, string funcName, StringBuilder fileContents, string exerciseId)
{
    public Guid ExecutionId => executionId;

    public string SigningKey => signingKey; // TODO maybe could just use ExecutionId?
    public Language Lang => lang;

    public string FuncName => funcName;

    public StringBuilder FileContents => fileContents;

    public string ExerciseId => exerciseId;
}