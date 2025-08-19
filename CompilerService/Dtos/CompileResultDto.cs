namespace CompilerService.Dtos;

public class CompileResultDto
{
    public Dictionary<string, string> GeneratedClassFiles { get; set; } = [];
}