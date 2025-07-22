using System.Diagnostics;
using System.Text;
using AnalyzerWip.Analyzer.AstAnalyzer;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Errors;
using ExecutorService.Executor._ExecutorUtils;
using ExecutorService.Executor.Configs;

namespace ExecutorService.Executor;

public interface ICodeExecutorService
{
    public Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto);
    public Task<ExecuteResultDto> DryExecute(ExecuteRequestDto executeRequestDto);
}


public class CodeExecutorService(
    IExecutorRepository executorRepository,
    ICompilationHandler compilationHandler
    ) : ICodeExecutorService
{
    private const string JavaImport = "import com.google.gson.Gson;\n\n"; // TODO this is temporary, not the gson but the way it's imported

    private IAnalyzer? _analyzer;
    private CodeAnalysisResult? _codeAnalysisResult;

    public async Task<ExecuteResultDto> FullExecute(ExecuteRequestDto executeRequestDto)
    {
        var lang = CheckLanguageSupported(executeRequestDto.Lang);
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, executeRequestDto.ExerciseId);

        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString());
        _codeAnalysisResult = _analyzer.AnalyzeUserCode();
        
        if (!_codeAnalysisResult.PassedValidation)
        {
            return new ExecuteResultDto("", "critical function signature modified. Exiting.");
        }
        
        if (_codeAnalysisResult.MainMethodIndices != null)
        {
            await InsertTestCases(fileData, _codeAnalysisResult.MainMethodIndices.MethodFileEndIndex);
        }
        else
        {
            // TODO temporary solution I'd like to insert a main if it's not found to test either way
            return new ExecuteResultDto("", "no main function found. Exiting");
        }

        return await Exec(fileData);
    }

    public async Task<ExecuteResultDto> DryExecute(ExecuteRequestDto executeRequestDto)
    {
        var lang = CheckLanguageSupported(executeRequestDto.Lang);
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, executeRequestDto.ExerciseId);
        
        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString());
        _codeAnalysisResult = _analyzer.AnalyzeUserCode();

        return await Exec(fileData);
    }

    
    // TODO add better error handling here
    private async Task<ExecuteResultDto> Exec(UserSolutionData userSolutionData)
    {
        var responseBytes = await CompileCode(userSolutionData);
        await File.WriteAllBytesAsync($"/tmp/{userSolutionData.ExecutionId}.class", responseBytes);

        await BuildCopyFs(userSolutionData);
        
        return await DispatchExecutorVm(userSolutionData);
    }

    private async Task<byte[]> CompileCode(UserSolutionData userSolutionData)
    {
        var codeBytes = Encoding.UTF8.GetBytes(userSolutionData.FileContents.ToString());
        var codeB64 = Convert.ToBase64String(codeBytes);
        
        return await compilationHandler.CompileAsync(codeB64, _codeAnalysisResult!.MainClassName);
    }
    
    private async Task BuildCopyFs(UserSolutionData userSolutionData)
    {
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/build-copy.sh \"{_codeAnalysisResult!.MainClassName}\" \"{userSolutionData.ExecutionId}\" \"{userSolutionData.SigningKey}\"", 
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
        
        buildProcess.Start();
        await buildProcess.WaitForExitAsync();
    }

    private async Task<ExecuteResultDto> DispatchExecutorVm(UserSolutionData userSolutionData)
    {
        var execProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/java-exec.sh {userSolutionData.ExecutionId} {userSolutionData.SigningKey}",
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };

        execProcess.Start();
        await execProcess.WaitForExitAsync();

        ExecuteResultDto executeResult = new();
        try
        {
            executeResult.StdOutput = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-OUT-LOG.log");
        }
        catch (FileNotFoundException ex)
        {
            // TODO handle this
        }
        try
        {
            var testResults = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-ANSW-LOG.log");
            executeResult.TestResults = testResults.Replace($"ctr-{userSolutionData.SigningKey}-ans: ", ""); // TODO could move this to the bash script ig
        }
        catch (FileNotFoundException e)
        {
            // TODO handle this
        }

        return executeResult;

    }
    
    private async Task<UserSolutionData> PrepareFile(string codeB64, string lang, string exerciseId) // TODO Make the import not constant
    {
        var codeBytes = Convert.FromBase64String(codeB64);
        var codeString = Encoding.UTF8.GetString(codeBytes);

        var code = new StringBuilder(codeString);
        
        var fileData = new UserSolutionData(Guid.NewGuid(), Guid.NewGuid().ToString(), lang, await executorRepository.GetFuncName(), code, exerciseId);
        code.Insert(0, JavaImport);
        
        return fileData;
    }

    private async Task InsertTestCases(UserSolutionData userSolutionData, int writeOffset)
    {
        var testCases = await executorRepository.GetTestCasesAsync();
        
        var testCaseInsertBuilder = new StringBuilder();
        testCaseInsertBuilder.Append("Gson gson = new Gson();\n");
        foreach (var testCase in testCases)
        {
            var comparingStatement = $"System.out.println(\"ctr-{userSolutionData.SigningKey}-ans: \" + gson.toJson({testCase.ExpectedOutput}).equals(gson.toJson(sortIntArr({testCase.TestInput}))));\n";
            testCaseInsertBuilder.Append(comparingStatement);
        }
        
        userSolutionData.FileContents.Insert(writeOffset, testCaseInsertBuilder);
    }

    private string CheckLanguageSupported(string lang)
    {
        var executorYmlConfig = YmlConfigReader.ReadExecutorYmlConfig();

        return executorYmlConfig.SUPPORTED_LANGUAGES.FirstOrDefault(l => l.ToLower().Equals(lang)) ??
            throw new LanguageException($"Language: {lang} not supported");
    }
}
