namespace AlgoDuck.Modules.Problem.DTOs.ExecutorDtos;

public class ExecuteRequestDto(string codeB64, string lang, string exerciseId)
{
    public string CodeB64 => codeB64;

    public string Lang => lang;
    
    public string ExerciseId => exerciseId;
}