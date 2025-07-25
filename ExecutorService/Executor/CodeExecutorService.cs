using System.Diagnostics;
using System.Text;
using ExecutorService.Analyzer._AnalyzerUtils;
using ExecutorService.Analyzer.AstAnalyzer;
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
        var lang = await CheckLanguageSupported(executeRequestDto.Lang);
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, executeRequestDto.ExerciseId);

        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString(), await executorRepository.GetTemplateAsync("0fd5d3a8-48c1-451b-bcdf-cf414cc6d477"));
        _codeAnalysisResult = _analyzer.AnalyzeUserCode();
        
        if (!_codeAnalysisResult.PassedValidation)
        {
            return new ExecuteResultDto("", "Template modified. Cannot proceed with testing. Exiting.", "");
        }
        
        if (_codeAnalysisResult.MainMethodIndices != null)
        {
            await InsertTestCases(fileData, _codeAnalysisResult.MainMethodIndices.MethodFileEndIndex);
        }
        else
        {
            // TODO temporary solution I'd like to insert a main if it's not found to test either way
            return new ExecuteResultDto("", "no main function found. Exiting", "");
        }

        return await Exec(fileData);
    }

    public async Task<ExecuteResultDto> DryExecute(ExecuteRequestDto executeRequestDto)
    {
        var lang = await CheckLanguageSupported(executeRequestDto.Lang);
        
        var fileData = await PrepareFile(executeRequestDto.CodeB64, lang, executeRequestDto.ExerciseId);
        
        _analyzer = new AnalyzerSimple(fileData.FileContents.ToString());
        _codeAnalysisResult = _analyzer.AnalyzeUserCode();

        return await Exec(fileData);
    }

    
    // TODO add better error handling here
    private async Task<ExecuteResultDto> Exec(UserSolutionData userSolutionData)
    {
        var compilationTask = CompileCode(userSolutionData);
        var fsCopyTask = CopyTemplateFs(userSolutionData);
        
        Task.WaitAll(compilationTask, fsCopyTask);

        await PopulateCopyFs(userSolutionData, await compilationTask);
        
        return await DispatchExecutorVm(userSolutionData);
    }

    private Task<byte[]> CompileCode(UserSolutionData userSolutionData)
    {
        var codeBytes = Encoding.UTF8.GetBytes(userSolutionData.FileContents.ToString());
        var codeB64 = Convert.ToBase64String(codeBytes);
        
        return compilationHandler.CompileAsync(codeB64, _codeAnalysisResult!.MainClassName);
    }

    private async Task<ExecuteResultDto> DispatchExecutorVm(UserSolutionData userSolutionData)
    {
        await JavaExecute(userSolutionData);

        ExecuteResultDto executeResult = new();
        try
        {
            executeResult.StdOutput = await File.ReadAllTextAsync($"/tmp/{userSolutionData.ExecutionId}-OUT-LOG.log");
        }
        catch (FileNotFoundException ex)
        {
            executeResult.StdOutput = ex.Message;
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
        //TODO hardcoded, change this
        var testCases = await executorRepository.GetTestCasesAsync("0fd5d3a8-48c1-451b-bcdf-cf414cc6d477");
        
        var testCaseInsertBuilder = new StringBuilder();
        testCaseInsertBuilder.Append("Gson gson = new Gson();\n");
        foreach (var testCase in testCases)
        {
            var comparingStatement = $"System.out.println(\"ctr-{userSolutionData.SigningKey}-ans: \" + gson.toJson({testCase.ExpectedOutput}).equals(gson.toJson(sortIntArr({testCase.TestInput}))));\n";
            testCaseInsertBuilder.Append(comparingStatement);
        }
        
        userSolutionData.FileContents.Insert(writeOffset, testCaseInsertBuilder);
    }

    private async Task<string> CheckLanguageSupported(string lang)
    {
        var supportedLanguages = await executorRepository.GetSupportedLanguagesAsync();
        
        return supportedLanguages.FirstOrDefault(l => l.Name.ToLower().Equals(lang))?.Name ??
               throw new LanguageException($"Language: {lang} not supported");
    }

    private Task CopyTemplateFs(UserSolutionData userSolutionData)
    {
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/copy-template-fs.sh \"{_codeAnalysisResult!.MainClassName}\" \"{userSolutionData.ExecutionId}\" \"{userSolutionData.SigningKey}\"", 
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
        
        buildProcess.Start();
        return buildProcess.WaitForExitAsync();
    }    
    
    private async Task PopulateCopyFs(UserSolutionData userSolutionData, byte[] userByteCode)
    {
        await File.WriteAllBytesAsync($"/tmp/{userSolutionData.ExecutionId}.class", userByteCode);
        
        var buildProcess = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "/bin/bash",
                Arguments = $"/app/fc-scripts/populate-execution-fs.sh \"{_codeAnalysisResult!.MainClassName}\" \"{userSolutionData.ExecutionId}\"", 
                RedirectStandardError = true,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                CreateNoWindow = true
            }
        };
        
        buildProcess.Start();
        await buildProcess.WaitForExitAsync();
    }

    private async Task JavaExecute(UserSolutionData userSolutionData)
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
    }
    
}
