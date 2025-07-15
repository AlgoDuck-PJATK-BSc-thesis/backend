namespace CompilerService.Dtos;

public class CompileRequestDto(string srcCodeB64, string classname)
{
    public string SrcCodeB64 => srcCodeB64;
    public string ClassName => classname;
}